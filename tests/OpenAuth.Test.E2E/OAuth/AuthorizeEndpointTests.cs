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
        await fixture.ResetAsync(_host);
    }
    
    public async Task DisposeAsync() => await _host.DisposeAsync();
    

    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithValidRequest_RedirectsWithCode()
    {
        var module = await _host.CreateApiClientAsync();

        var response = await module.AuthorizeAsync(opts =>
            opts.WithClient(module.Id));
        
        Assert.True(response.Success);
        Assert.StartsWith(DefaultValues.RedirectUri, response.RedirectUri, StringComparison.OrdinalIgnoreCase);
        
        Assert.NotNull(response.Code);
        Assert.NotEmpty(response.Code);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithState_RedirectsWithState()
    {
        const string state = "12345";
        
        var module = await _host.CreateApiClientAsync();
        
        var response = await module.AuthorizeAsync(opts =>
        {
            opts.WithClient(module.Id);
            opts.WithState(state);
        });
        
        Assert.True(response.Success);
        Assert.StartsWith(DefaultValues.RedirectUri, response.RedirectUri, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(state, response.State);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithInvalidClient_ReturnsBadRequest()
    {
        var module = await _host.CreateApiClientAsync();
        
        var response = await module.AuthorizeAsync(opts =>
            opts.WithClient("invalid-client-id"));
        
        Assert.False(response.Success);
        Assert.Null(response.RedirectUri);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithInvalidRedirectUri_ReturnsBadRequest()
    {
        var module = await _host.CreateApiClientAsync();

        var response = await module.AuthorizeAsync(opts =>
        {
            opts.WithClient(module.Id);
            opts.WithRedirectUri("https://invalid-redirect.com");
        });
        
        Assert.False(response.Success);
        Assert.Null(response.RedirectUri);
    }
    
    [Fact]
    public async Task GivenAuthenticatedUser_WhenAuthorizeIsCalled_WithInvalidResponseType_ReturnsBadRequest()
    {
        var module = await _host.CreateApiClientAsync();
        
        var response = await module.AuthorizeAsync(opts =>
        {
            opts.WithClient(module.Id);
            opts.WithResponseType("banana");
        });
        
        Assert.False(response.Success);
        Assert.NotNull(response.RedirectUri);
        Assert.Contains(response.Error!, "unsupported_response_type");
    }
}