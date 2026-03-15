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
		await fixture.ResetAsync(_host);
	}

	public async Task DisposeAsync() => await _host.DisposeAsync();


	[Fact]
	public async Task AuthorizationCodeFlow_WhenValid_Succeeds()
	{
		var module = await _host.CreateClientAsync();
		await module.AuthorizeAsync();
		
		var result = await module.ExchangeCodeForTokenAsync();
		
		Assert.NotNull(result.AccessToken);
	}
	
	[Fact]
	public async Task AuthorizationCodeFlow_WhenInvalidClientSecret_ThrowsInvalidClientException()
	{
		var module = await _host.CreateClientAsync();
		await module.AuthorizeAsync();
	   
		await Assert.ThrowsAsync<InvalidClientException>(()
			=> module.ExchangeCodeForTokenAsync(opts =>
				opts.WithClientSecret("invalid-client-secret")));
	}
	
	[Fact]
	public async Task AuthorizationCodeFlow_WhenClientIsPublic_ThrowsException()
	{
		var module = await _host.CreateClientAsync(opts =>
			opts.WithApplicationType("spa"));
	   
		await Assert.ThrowsAsync<InvalidRequestException>(()
			=> module.AuthorizeAsync());
	}
	
	[Fact]
	public async Task AuthorizationCodeFlowWithPkce_WhenValid_Succeeds()
	{
		var module = await _host.CreateClientAsync(opts =>
			opts.WithApplicationType("spa"));
		
		var (verifier, pkce) = PkceHelpers.Create();
	       
		await module.AuthorizeAsync(opts =>
			opts.WithPkce(pkce));
	       
		var result = await module.ExchangeCodeForTokenAsync(opts =>
			opts.WithCodeVerifier(verifier));
	
		Assert.NotNull(result.AccessToken);
	}
	
	[Fact]
	public async Task AuthorizationCodeFlowWithPkce_WhenInvalidCodeVerifier_ThrowsInvalidGrantException()
	{
		var module = await _host.CreateClientAsync(opts =>
			opts.WithApplicationType("spa"));
		
		var (_, pkce) = PkceHelpers.Create();
	
		await module.AuthorizeAsync(opts =>
			opts.WithPkce(pkce));
	   
		await Assert.ThrowsAsync<InvalidGrantException>(() =>
			module.ExchangeCodeForTokenAsync(opts =>
				opts.WithCodeVerifier("invalid-code-verifier")));
	}
}