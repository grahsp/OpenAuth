using OpenAuth.Test.Common.Helpers;
using OpenAuth.Test.Integration.Infrastructure.Fixtures;

namespace OpenAuth.Test.Integration.OAuth;

[Collection("sqlserver")]
public class AuthorizeEndpointTests(ApiServerFixture fx) : IClassFixture<ApiServerFixture>, IAsyncLifetime
{
    public Task InitializeAsync() => fx.InitializeAsync();
    public Task DisposeAsync() => Task.CompletedTask;
    

    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithValidRequest_RedirectsWithCode()
    {
        var client = await fx.CreateClientAsync();

        var response = await client.AuthorizeAsync(opts =>
            opts.WithClient(client.Id));
        
        Assert.True(response.Success);
        Assert.StartsWith(DefaultValues.RedirectUri, response.RedirectUri, StringComparison.OrdinalIgnoreCase);
        
        Assert.NotNull(response.Code);
        Assert.NotEmpty(response.Code);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithState_RedirectsWithState()
    {
        const string state = "12345";
        
        var client = await fx.CreateClientAsync();
        
        var response = await client.AuthorizeAsync(opts =>
        {
            opts.WithClient(client.Id);
            opts.WithState(state);
        });
        
        Assert.True(response.Success);
        Assert.StartsWith(DefaultValues.RedirectUri, response.RedirectUri, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(state, response.State);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithInvalidClient_ReturnsBadRequest()
    {
        var client = await fx.CreateClientAsync();
        
        var response = await client.AuthorizeAsync(opts =>
            opts.WithClient("invalid-client-id"));
        
        Assert.False(response.Success);
        Assert.Null(response.RedirectUri);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithInvalidRedirectUri_ReturnsBadRequest()
    {
        var client = await fx.CreateClientAsync();

        var response = await client.AuthorizeAsync(opts =>
        {
            opts.WithClient(client.Id);
            opts.WithRedirectUri("https://invalid-redirect.com");
        });
        
        Assert.False(response.Success);
        Assert.Null(response.RedirectUri);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithInvalidResponseType_ReturnsBadRequest()
    {
        var client = await fx.CreateClientAsync();
        
        var response = await client.AuthorizeAsync(opts =>
        {
            opts.WithClient(client.Id);
            opts.WithResponseType("banana");
        });
        
        Assert.False(response.Success);
        Assert.NotNull(response.RedirectUri);
        Assert.Contains(response.Error!, "unsupported_response_type");
    }
}