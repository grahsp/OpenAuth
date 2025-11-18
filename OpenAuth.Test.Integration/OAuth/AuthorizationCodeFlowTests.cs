using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Integration.Infrastructure.Clients;
using OpenAuth.Test.Integration.Infrastructure.Fixtures;

namespace OpenAuth.Test.Integration.OAuth;

[Collection("sqlserver")]
public class AuthorizationCodeFlowTests : IClassFixture<TestApplicationFixture>, IAsyncLifetime
{
    private readonly TestClient _app;
    private readonly TestApplicationFixture _fx;

    public AuthorizationCodeFlowTests(TestApplicationFixture fx)
    {
        _app = new TestClient(fx);
        _fx = fx;
    }

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;


    [Fact]
    public async Task AuthorizationCodeFlow_WhenValid_Succeeds()
    {
        const string redirectUri = "https://example.com/callback";
        var registered = await _app.Clients
            .Web()
            .WithRedirectUri(redirectUri)
            .CreateAsync();

        var authorizeCommand = new AuthorizeCommandBuilder(registered.Client.Id, redirectUri)
            .Build();
        
        var authorizationGrant = await _app.AuthorizeAsync(authorizeCommand);

        var tokenRequest = TokenRequestBuilderFactory
            .BuildAuthorizationCodeRequest(registered.Client.Id, authorizationGrant)
            .WithClientSecret(registered.ClientSecret!)
            .Build();
        
        var tokenResponse = await _app.RequestTokenAsync(tokenRequest);
        
        Assert.NotNull(tokenResponse.Token);
    }
    
    [Fact]
    public async Task AuthorizationCodeFlow_WhenInvalidClientSecret_ThrowsException()
    {
        const string redirectUri = "https://example.com/callback";
        var registered = await _app.Clients
            .Web()
            .WithRedirectUri(redirectUri)
            .CreateAsync();

        var authorizeCommand = new AuthorizeCommandBuilder(registered.Client.Id, redirectUri)
            .Build();
        
        var authorizationGrant = await _app.AuthorizeAsync(authorizeCommand);

        var tokenRequest = TokenRequestBuilderFactory
            .BuildAuthorizationCodeRequest(registered.Client.Id, authorizationGrant)
            .WithClientSecret("invalid-client-secret")
            .Build();
        
        await Assert.ThrowsAsync<InvalidOperationException>(()
            => _app.RequestTokenAsync(tokenRequest));
    }
    
    [Fact]
    public async Task AuthorizationCodeFlow_WhenClientIsPublic_ThrowsException()
    {
        const string redirectUri = "https://example.com/callback";
        var registered = await _app.Clients
            .Spa()
            .WithRedirectUri(redirectUri)
            .CreateAsync();

        var authorizeCommand = new AuthorizeCommandBuilder(registered.Client.Id, redirectUri)
            .Build();
        
        await Assert.ThrowsAsync<InvalidOperationException>(()
            => _app.AuthorizeAsync(authorizeCommand));
    }
    
    [Fact]
    public async Task AuthorizationCodeFlowWithPkce_WhenValid_Succeeds()
    {
        const string redirectUri = "https://example.com/callback";
        var registered = await _app.Clients
            .Spa()
            .WithRedirectUri(redirectUri)
            .CreateAsync();
        
        const string codeVerifier = "this-is-a-secret-code";
        var pkce = Pkce.FromVerifier(codeVerifier, "s256");

        var authorizeCommand = new AuthorizeCommandBuilder(registered.Client.Id, redirectUri)
            .WithPkce(pkce)
            .Build();
        var authorizationGrant = await _app.AuthorizeAsync(authorizeCommand);

        var tokenRequest = TokenRequestBuilderFactory
            .BuildAuthorizationCodeRequest(registered.Client.Id, authorizationGrant)
            .WithCodeVerifier(codeVerifier)
            .Build();
        var tokenResponse = await _app.RequestTokenAsync(tokenRequest);
        
        Assert.NotNull(tokenResponse.Token);
    }
    
    [Fact]
    public async Task AuthorizationCodeFlowWithPkce_WhenInvalidCodeVerifier_ThrowsException()
    {
        const string redirectUri = "https://example.com/callback";
        var registered = await _app.Clients
            .Spa()
            .WithRedirectUri(redirectUri)
            .CreateAsync();
        
        const string codeVerifier = "this-is-a-secret-code";
        var pkce = Pkce.FromVerifier(codeVerifier, "s256");

        var authorizeCommand = new AuthorizeCommandBuilder(registered.Client.Id, redirectUri)
            .WithPkce(pkce)
            .Build();
        
        var authorizationGrant = await _app.AuthorizeAsync(authorizeCommand);

        var tokenRequest = TokenRequestBuilderFactory
            .BuildAuthorizationCodeRequest(registered.Client.Id, authorizationGrant)
            .WithCodeVerifier("invalid-code-verifier")
            .Build();
        
        await Assert.ThrowsAsync<InvalidOperationException>(()
            => _app.RequestTokenAsync(tokenRequest));
    }
}