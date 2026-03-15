using OpenAuth.Application.Exceptions;
using OpenAuth.Test.Common.Fixtures;
using OpenAuth.Test.Common.Hosting;
using OpenAuth.Test.Integration.Extensions;

namespace OpenAuth.Test.Integration.OAuth;

public class ClientCredentialsFlowTests(TestFixture fixture) : IClassFixture<TestFixture>, IAsyncLifetime
{
    private TestHost _host = null!;

    public async Task InitializeAsync() {
        _host = fixture.CreateDefaultHost();
        await fixture.ResetAsync();
        
        await _host.SeedSigningKeyAsync();
    }
    
    public async Task DisposeAsync() => await _host.DisposeAsync();


    [Fact]
    public async Task ClientCredentialsFlow_WhenValid_Succeeds()
    {
        var client = await _host.CreateClientAsync(opts =>
            opts.WithApplicationType("m2m"));
        
        var result = await client.RequestClientCredentialsTokenAsync();
        
        Assert.NotNull(result.AccessToken);
    }
     
    [Fact]
    public async Task ClientCredentialsFlow_WhenInvalidClientSecret_ThrowsInvalidClientException()
    {
        var client = await _host.CreateClientAsync(opts =>
            opts.WithApplicationType("m2m"));
    
        await Assert.ThrowsAsync<InvalidClientException>(()
            => client.RequestClientCredentialsTokenAsync(config =>
                config.WithClientSecret("invalid-client-secret")
            ));
    }
}