using OpenAuth.Application.Exceptions;
using OpenAuth.Test.Common.Fixtures;
using OpenAuth.Test.Common.Helpers;
using OpenAuth.Test.Common.Hosting;
using OpenAuth.Test.Integration.Extensions;

namespace OpenAuth.Test.Integration.OAuth;

public class AuthorizationCodeFlowTests(TestFixture fixture) : IClassFixture<TestFixture>, IAsyncLifetime
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
	public async Task AuthorizationCodeFlow_WhenValid_Succeeds()
	{
		var client = await _host.CreateClientAsync();
		await client.AuthorizeAsync();
		
		var result = await client.ExchangeCodeForTokenAsync();
		
		Assert.NotNull(result.AccessToken);
	}
	
	[Fact]
	public async Task AuthorizationCodeFlow_WhenInvalidClientSecret_ThrowsInvalidClientException()
	{
		var client = await _host.CreateClientAsync();
		await client.AuthorizeAsync();
	   
		await Assert.ThrowsAsync<InvalidClientException>(()
			=> client.ExchangeCodeForTokenAsync(opts =>
				opts.WithClientSecret("invalid-client-secret")));
	}
	
	[Fact]
	public async Task AuthorizationCodeFlow_WhenClientIsPublic_ThrowsException()
	{
		var client = await _host.CreateClientAsync(opts =>
			opts.WithApplicationType("spa"));
	   
		await Assert.ThrowsAsync<InvalidRequestException>(()
			=> client.AuthorizeAsync());
	}
	
	[Fact]
	public async Task AuthorizationCodeFlowWithPkce_WhenValid_Succeeds()
	{
		var (verifier, pkce) = PkceHelpers.Create();
		var client = await _host.CreateClientAsync(opts =>
			opts.WithApplicationType("spa"));
	       
		await client.AuthorizeAsync(opts =>
			opts.WithPkce(pkce));
	       
		var result = await client.ExchangeCodeForTokenAsync(opts =>
			opts.WithCodeVerifier(verifier));
	
		Assert.NotNull(result.AccessToken);
	}
	
	[Fact]
	public async Task AuthorizationCodeFlowWithPkce_WhenInvalidCodeVerifier_ThrowsInvalidGrantException()
	{
		var (_, pkce) = PkceHelpers.Create();
		var client = await _host.CreateClientAsync(opts =>
			opts.WithApplicationType("spa"));
	
		await client.AuthorizeAsync(opts =>
			opts.WithPkce(pkce));
	   
		await Assert.ThrowsAsync<InvalidGrantException>(() =>
			client.ExchangeCodeForTokenAsync(opts =>
				opts.WithCodeVerifier("invalid-code-verifier")));
	}
}