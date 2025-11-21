using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Test.Integration.Infrastructure.Fixtures;

namespace OpenAuth.Test.Integration.OAuth;

[Collection("sqlserver")]
public class ClientCredentialsFlowTests : IClassFixture<ApplicationFixture>, IAsyncLifetime
{
    private readonly ApplicationFixture _fx;

    public ClientCredentialsFlowTests(ApplicationFixture fx) =>
        _fx = fx;

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;


    [Fact]
    public async Task ClientCredentialsFlow_WhenValid_Succeeds()
    {
        var client = await _fx.CreateClientAsync(ClientApplicationTypes.M2M);
        
        var result = await client.RequestClientCredentialsTokenAsync();
        
        Assert.NotNull(result.Token);
    }
     
    [Fact]
    public async Task ClientCredentialsFlow_WhenInvalidClientSecret_Fails()
    {
        var client = await _fx.CreateClientAsync(ClientApplicationTypes.M2M);
    
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()
            => client.RequestClientCredentialsTokenAsync(config =>
                config.WithClientSecret("invalid-client-secret")
            ));
    }
}