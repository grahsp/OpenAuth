using OpenAuth.Application.OAuth.Stores;
using OpenAuth.Test.Common.Helpers;
using OpenAuth.Test.Integration.Infrastructure.Fixtures;

namespace OpenAuth.Test.Integration.OAuth;

[Collection("sqlserver")]
public class TokenEndpointTests(ApiServerFixture fx) : IClassFixture<ApiServerFixture>, IAsyncLifetime
{
    public Task InitializeAsync() => fx.InitializeAsync();
    public Task DisposeAsync() => Task.CompletedTask;


    [Fact]
    public async Task ClientCredentials_Success()
    {
        var client = await fx.CreateClientAsync(opts =>
            opts.WithApplicationType("m2m"));

        var response = await client.RequestTokenAsync(opts =>
        {
            opts.WithGrantType("client_credentials");
            opts.WithAudience(DefaultValues.Audience);
            opts.WithScopes(DefaultValues.Scopes);
        });

        Assert.NotNull(response);
        Assert.NotNull(response.AccessToken);
        Assert.Null(response.Error);
    }
    
    [Fact]
    public async Task ClientCredentials_WhenInvalidSecret_ReturnsInvalidClientError()
    {
        var client = await fx.CreateClientAsync(opts =>
            opts.WithApplicationType("m2m"));

        var response = await client.RequestTokenAsync(opts =>
        {
            opts.WithGrantType("client_credentials");
            opts.WithAudience(DefaultValues.Audience);
            opts.WithScopes(DefaultValues.Scopes);
            opts.WithClientSecret("invalid-client-secret");
        });

        Assert.NotNull(response);
        Assert.Equal("invalid_client", response.Error);
    }

    [Fact]
    public async Task AuthorizationCode_Success()
    {
        var client = await fx.CreateClientAsync(opts =>
            opts.WithApplicationType("web"));

        var grant = await client.AuthorizeAsync();

        var response = await client.RequestTokenAsync(opts =>
        {
            opts.WithGrantType("authorization_code");
            opts.WithCode(grant.Code);
            opts.WithClientId(client.Id);
            opts.WithRedirectUri(DefaultValues.RedirectUri);
            opts.WithAudience(DefaultValues.Audience);
            opts.WithScopes(DefaultValues.Scopes);
            opts.WithClientSecret(client.Secret);
        });

        Assert.NotNull(response);
        Assert.NotNull(response.AccessToken);
        Assert.Null(response.Error);
    }
    
    [Fact]
    public async Task AuhtorizationCode_WhenInvalidRedirectUri_ReturnsInvalidGrantError()
    {
        var client = await fx.CreateClientAsync(opts =>
            opts.WithApplicationType("web"));

        var grant = await client.AuthorizeAsync();

        var response = await client.RequestTokenAsync(opts =>
        {
            opts.WithGrantType("authorization_code");
            opts.WithCode(grant.Code);
            opts.WithClientId(client.Id);
            opts.WithRedirectUri("https://invalid-uri.com");
            opts.WithAudience(DefaultValues.Audience);
            opts.WithScopes(DefaultValues.Scopes);
            opts.WithClientSecret(client.Secret);
        });

        Assert.NotNull(response);
        Assert.Equal("invalid_grant", response.Error);
        Assert.Contains("redirect_uri", response.ErrorDescription);
    }

    [Fact]
    public async Task AuthorizationCodePkce_Success()
    {
        var client = await fx.CreateClientAsync(opts =>
            opts.WithApplicationType("spa"));

        var (verifier, pkce) = PkceHelpers.Create();

        var grant = await client.AuthorizeAsync(opts =>
            opts.WithPkce(pkce.CodeChallenge, pkce.CodeChallengeMethod.ToString()));

        var response = await client.RequestTokenAsync(opts =>
        {
            opts.WithGrantType("authorization_code");
            opts.WithCode(grant.Code);
            opts.WithClientId(client.Id);
            opts.WithRedirectUri(DefaultValues.RedirectUri);
            opts.WithAudience(DefaultValues.Audience);
            opts.WithScopes(DefaultValues.Scopes);
            opts.WithCodeVerifier(verifier);
        });

        Assert.NotNull(response);
        Assert.NotNull(response.AccessToken);
        Assert.Null(response.Error);
    }
    
    [Fact]
    public async Task AuthorizationCodePkce_WithInvalidVerifier_ReturnsInvalidGrantError()
    {
        var client = await fx.CreateClientAsync(opts =>
            opts.WithApplicationType("spa"));

        var (verifier, pkce) = PkceHelpers.Create();

        var grant = await client.AuthorizeAsync(opts =>
            opts.WithPkce(pkce.CodeChallenge, pkce.CodeChallengeMethod.ToString()));

        var response = await client.RequestTokenAsync(opts =>
        {
            opts.WithGrantType("authorization_code");
            opts.WithCode(grant.Code);
            opts.WithClientId(client.Id);
            opts.WithRedirectUri(DefaultValues.RedirectUri);
            opts.WithAudience(DefaultValues.Audience);
            opts.WithScopes(DefaultValues.Scopes);
            opts.WithCodeVerifier("invalid-code-verifier");
        });

        Assert.NotNull(response);
        Assert.Equal("invalid_grant", response.Error);
        Assert.Contains("code verifier", response.ErrorDescription);
    }
    
    [Fact]
    public async Task AuthorizationCodePkce_WithOidc_Success()
    {
        var client = await fx.CreateClientAsync(opts =>
            opts.WithApplicationType("spa"));

        var (verifier, pkce) = PkceHelpers.Create();

        var grant = await client.AuthorizeAsync(opts =>
        {
            opts.WithPkce(pkce.CodeChallenge, pkce.CodeChallengeMethod.ToString());
            opts.WithScope(DefaultValues.Scopes + " openid profile");
            opts.WithNonce("test-nonce");
        });

        var response = await client.RequestTokenAsync(opts =>
        {
            opts.WithGrantType("authorization_code");
            opts.WithCode(grant.Code);
            opts.WithClientId(client.Id);
            opts.WithRedirectUri(DefaultValues.RedirectUri);
            opts.WithAudience(DefaultValues.Audience);
            opts.WithScopes(DefaultValues.Scopes);
            opts.WithCodeVerifier(verifier);
        });

        Assert.NotNull(response);
        Assert.NotNull(response.AccessToken);
        Assert.NotNull(response.IdToken);
        Assert.Null(response.Error);
    }
}