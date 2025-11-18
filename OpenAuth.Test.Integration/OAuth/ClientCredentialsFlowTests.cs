using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Integration.Infrastructure.Clients;
using OpenAuth.Test.Integration.Infrastructure.Fixtures;

namespace OpenAuth.Test.Integration.OAuth;

[Collection("sqlserver")]
public class ClientCredentialsFlowTests : IClassFixture<TestApplicationFixture>, IAsyncLifetime
{
    private readonly TestClient _app;
    private readonly TestApplicationFixture _fx;

    public ClientCredentialsFlowTests(TestApplicationFixture fx)
    {
        _app = new TestClient(fx);
        _fx = fx;
    }

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;


    [Fact]
    public async Task ClientCredentialsFlow_WhenValid_Succeeds()
    {
        const string audience = "api";
        const string scopes = "read write";

        var registered = await _app.Clients
            .M2M()
            .WithPermission(audience, scopes)
            .CreateAsync();

        var tokenRequest = TokenRequestBuilderFactory
            .BuildClientCredentialsRequest(registered.Client.Id, registered.ClientSecret!)
            .WithAudience(audience)
            .WithScopes(scopes)
            .Build();

        var token = await _app.RequestTokenAsync(tokenRequest);
        
        Assert.NotNull(token);
    }

     
     [Fact]
     public async Task ClientCredentialsFlow_WhenInvalidClientSecret_Fails()
     {
         const string audience = "api";
         const string scopes = "read write";

        var registered = await _app.Clients
            .M2M()
            .WithPermission(audience, scopes)
            .CreateAsync();

        var tokenRequest = TokenRequestBuilderFactory
            .BuildClientCredentialsRequest(registered.Client.Id, "invalid-client-secret")
            .WithAudience(audience)
            .WithScopes(scopes)
            .Build();

         await Assert.ThrowsAsync<UnauthorizedAccessException>(()
             => _app.RequestTokenAsync(tokenRequest));
     }
}