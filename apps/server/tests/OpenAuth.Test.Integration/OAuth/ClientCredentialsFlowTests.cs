using OpenAuth.Application.Exceptions;
using OpenAuth.Test.Common.Fixtures;
using OpenAuth.Test.Common.Hosting;
using OpenAuth.Test.Integration.Extensions;

namespace OpenAuth.Test.Integration.OAuth;

[Collection("integration")]
public class ClientCredentialsFlowTests(TestFixture fixture) : IAsyncLifetime
{
	private readonly TestHost _host = fixture.CreateDefaultHost();
	public async Task InitializeAsync() => await fixture.ResetAsync(_host);
	public async Task DisposeAsync() => await _host.DisposeAsync();


	[Fact]
	public async Task ClientCredentialsFlow_WhenValid_Succeeds()
	{
		var module = await _host.CreateClientAsync(opts =>
			opts.WithApplicationType("m2m"));
        
		var result = await module.RequestClientCredentialsTokenAsync();
        
		Assert.NotNull(result.AccessToken);
	}
     
	[Fact]
	public async Task ClientCredentialsFlow_WhenInvalidClientSecret_ThrowsInvalidClientException()
	{
		var module = await _host.CreateClientAsync(opts =>
			opts.WithApplicationType("m2m"));
    
		await Assert.ThrowsAsync<InvalidClientException>(() =>
			module.RequestClientCredentialsTokenAsync(config =>
				config.WithClientSecret("invalid")));
	}
}