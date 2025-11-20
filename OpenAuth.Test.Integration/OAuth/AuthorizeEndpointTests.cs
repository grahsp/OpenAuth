using OpenAuth.Domain.Clients.ApplicationType;
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
        const string redirectUri = "https://google.com";
        
        var client = await fx.CreateClientAsync(ClientApplicationTypes.Web, opts
            => opts.WithRedirectUri(redirectUri));

        var response = await client.AuthorizeAsync(opts
            => opts.WithClient(client.Id)
                .WithRedirectUri(redirectUri));
        
        Assert.True(response.Success);
        Assert.StartsWith(redirectUri, response.RedirectUri, StringComparison.OrdinalIgnoreCase);
        
        Assert.NotNull(response.Code);
        Assert.NotEmpty(response.Code);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithState_RedirectsWithState()
    {
        const string redirectUri = "https://google.com";
        const string state = "12345";
        
        var client = await fx.CreateClientAsync(ClientApplicationTypes.Web, opts
            => opts.WithRedirectUri(redirectUri));
        
        var response = await client.AuthorizeAsync(query
            => query.WithClient(client.Id)
                .WithRedirectUri(redirectUri)
                .WithState(state));
        
        Assert.True(response.Success);
        Assert.StartsWith(redirectUri, response.RedirectUri, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(state, response.State);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithInvalidClient_ReturnsBadRequest()
    {
        const string redirectUri = "https://google.com";
        
        var client = await fx.CreateClientAsync(ClientApplicationTypes.Web, opts
            => opts.WithRedirectUri(redirectUri));
        
        var response = await client.AuthorizeAsync(opts
            => opts.WithClient("invalid-client-id")
                .WithRedirectUri(redirectUri));
        
        Assert.False(response.Success);
        Assert.Null(response.RedirectUri);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithInvalidRedirectUri_ReturnsBadRequest()
    {
        const string redirectUri = "https://google.com";
        
        var client = await fx.CreateClientAsync(ClientApplicationTypes.Web, opts
            => opts.WithRedirectUri(redirectUri));
        
        var response = await client.AuthorizeAsync(opts
            => opts.WithClient("invalid-client-id")
                .WithRedirectUri(redirectUri + "/callback"));
        
        Assert.False(response.Success);
        Assert.Null(response.RedirectUri);
    }
}