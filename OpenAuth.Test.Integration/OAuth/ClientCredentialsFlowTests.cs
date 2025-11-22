using OpenAuth.Application.Exceptions;
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
        var client = await _fx.CreateClientAsync(opts =>
            opts.WithApplicationType("m2m"));
        
        var result = await client.RequestClientCredentialsTokenAsync();
        
        Assert.NotNull(result.Token);
    }
     
    [Fact]
    public async Task ClientCredentialsFlow_WhenInvalidClientSecret_ThrowsInvalidClientException()
    {
        var client = await _fx.CreateClientAsync(opts =>
            opts.WithApplicationType("m2m"));
    
        await Assert.ThrowsAsync<InvalidClientException>(()
            => client.RequestClientCredentialsTokenAsync(config =>
                config.WithClientSecret("invalid-client-secret")
            ));
    }
}