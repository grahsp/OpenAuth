using OpenAuth.Application.Exceptions;
using OpenAuth.Test.Common.Helpers;
using OpenAuth.Test.Integration.Infrastructure.Fixtures;

namespace OpenAuth.Test.Integration.OAuth;

[Collection("sqlserver")]
public class AuthorizationCodeFlowTests : IClassFixture<ApplicationFixture>, IAsyncLifetime
{
    private readonly ApplicationFixture _fx;

    public AuthorizationCodeFlowTests(ApplicationFixture fx) =>
        _fx = fx;

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;


    [Fact]
    public async Task AuthorizationCodeFlow_WhenValid_Succeeds()
    {
        var client = await _fx.CreateClientAsync();
        
        await client.AuthorizeAsync();
        var result = await client.ExchangeCodeForTokenAsync();
        
        Assert.NotNull(result.Token);
    }
    
    [Fact]
    public async Task AuthorizationCodeFlow_WhenInvalidClientSecret_ThrowsInvalidClientException()
    {
        var client = await _fx.CreateClientAsync();
        
        await client.AuthorizeAsync();
    
        await Assert.ThrowsAsync<InvalidClientException>(()
            => client.ExchangeCodeForTokenAsync(opts =>
                opts.WithClientSecret("invalid-client-secret")
            ));
    }
    
    [Fact]
    public async Task AuthorizationCodeFlow_WhenClientIsPublic_ThrowsException()
    {
        var client = await _fx.CreateClientAsync(opts =>
            opts.WithApplicationType("spa"));
    
        await Assert.ThrowsAsync<InvalidRequestException>(()
            => client.AuthorizeAsync());
    }
    
    [Fact]
    public async Task AuthorizationCodeFlowWithPkce_WhenValid_Succeeds()
    {
        var (verifier, pkce) = PkceHelpers.Create();
        var client = await _fx.CreateClientAsync(opts =>
            opts.WithApplicationType("spa"));
        
        await client.AuthorizeAsync(opts =>
            opts.WithPkce(pkce));
        
        var result = await client.ExchangeCodeForTokenAsync(opts =>
            opts.WithCodeVerifier(verifier));
 
        Assert.NotNull(result.Token);
    }
    
    [Fact]
    public async Task AuthorizationCodeFlowWithPkce_WhenInvalidCodeVerifier_ThrowsInvalidGrantException()
    {
        var (_, pkce) = PkceHelpers.Create();
        var client = await _fx.CreateClientAsync(opts =>
            opts.WithApplicationType("spa"));

        await client.AuthorizeAsync(opts =>
            opts.WithPkce(pkce));
    
        await Assert.ThrowsAsync<InvalidGrantException>(() =>
            client.ExchangeCodeForTokenAsync(opts =>
                opts.WithCodeVerifier("invalid-code-verifier")));
    }
}