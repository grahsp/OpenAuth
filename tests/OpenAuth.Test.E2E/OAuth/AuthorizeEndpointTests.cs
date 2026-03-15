using OpenAuth.Test.Common.Fixtures;
using OpenAuth.Test.Common.Helpers;
using OpenAuth.Test.Common.Hosting;
using OpenAuth.Test.E2E.Extensions;

namespace OpenAuth.Test.E2E.OAuth;

public class AuthorizeEndpointTests(TestFixture fixture) : IClassFixture<TestFixture>, IAsyncLifetime
{
    private TestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = fixture.CreateDefaultHost();
        await fixture.ResetAsync();

        await _host.SeedSigningKeyAsync();
    }
    
    public async Task DisposeAsync() => await _host.DisposeAsync();
    

    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithValidRequest_RedirectsWithCode()
    {
        var client = await _host.CreateApiClientAsync();

        var response = await client.AuthorizeAsync(opts =>
            opts.WithClient(client.Id));
        
        Assert.True((bool)response.Success);
        Assert.StartsWith((string?)DefaultValues.RedirectUri, (string?)response.RedirectUri, StringComparison.OrdinalIgnoreCase);
        
        Assert.NotNull(response.Code);
        Assert.NotEmpty(response.Code);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithState_RedirectsWithState()
    {
        const string state = "12345";
        
        var client = await _host.CreateApiClientAsync();
        
        var response = await client.AuthorizeAsync(opts =>
        {
            opts.WithClient(client.Id);
            opts.WithState(state);
        });
        
        Assert.True((bool)response.Success);
        Assert.StartsWith((string?)DefaultValues.RedirectUri, (string?)response.RedirectUri, StringComparison.OrdinalIgnoreCase);
        Assert.Equal((string?)state, (string?)response.State);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithInvalidClient_ReturnsBadRequest()
    {
        var client = await _host.CreateApiClientAsync();
        
        var response = await client.AuthorizeAsync(opts =>
            opts.WithClient("invalid-client-id"));
        
        Assert.False((bool)response.Success);
        Assert.Null(response.RedirectUri);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithInvalidRedirectUri_ReturnsBadRequest()
    {
        var client = await _host.CreateApiClientAsync();

        var response = await client.AuthorizeAsync(opts =>
        {
            opts.WithClient(client.Id);
            opts.WithRedirectUri("https://invalid-redirect.com");
        });
        
        Assert.False((bool)response.Success);
        Assert.Null(response.RedirectUri);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithInvalidResponseType_ReturnsBadRequest()
    {
        var client = await _host.CreateApiClientAsync();
        
        var response = await client.AuthorizeAsync(opts =>
        {
            opts.WithClient(client.Id);
            opts.WithResponseType("banana");
        });
        
        Assert.False((bool)response.Success);
        Assert.NotNull(response.RedirectUri);
        Assert.Contains((string)response.Error!, "unsupported_response_type");
    }
}