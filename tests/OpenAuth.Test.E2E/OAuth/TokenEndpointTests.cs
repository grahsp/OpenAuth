using OpenAuth.Test.Common.Fixtures;
using OpenAuth.Test.Common.Helpers;
using OpenAuth.Test.Common.Hosting;
using OpenAuth.Test.E2E.Extensions;
using Given = OpenAuth.Test.Common.Helpers.Given;

namespace OpenAuth.Test.E2E.OAuth;

[Collection("integration")]
public class TokenEndpointTests(TestFixture fixture) : IAsyncLifetime
{
	private readonly TestHost _host = fixture.CreateDefaultHost();
	public async Task InitializeAsync() => await fixture.ResetAsync(_host);
	public async Task DisposeAsync() => await _host.DisposeAsync();


	[Fact]
	public async Task ClientCredentials_Success()
	{
		var module = await _host.CreateApiClientAsync(opts =>
			opts.WithApplicationType("m2m"));

		var response = await module.RequestTokenAsync(opts =>
		{
			opts.WithGrantType("client_credentials");
			opts.WithAudience(DefaultValues.ApiAudience);
			opts.WithScopes(DefaultValues.Scopes);
		});

		Assert.NotNull(response);
		Assert.NotNull(response.AccessToken);
		Assert.Null(response.Error);
	}
    
	[Fact]
	public async Task ClientCredentials_WhenInvalidSecret_ReturnsInvalidClientError()
	{
		var module = await _host.CreateApiClientAsync(opts =>
			opts.WithApplicationType("m2m"));

		var response = await module.RequestTokenAsync(opts =>
		{
			opts.WithGrantType("client_credentials");
			opts.WithAudience(DefaultValues.ApiAudience);
			opts.WithScopes(DefaultValues.Scopes);
			opts.WithClientSecret("invalid-client-secret");
		});

		Assert.NotNull(response);
		Assert.Equal((string?)"invalid_client", (string?)response.Error);
	}

	[Fact]
	public async Task AuthorizationCode_Success()
	{
		var module = await _host.CreateApiClientAsync(opts =>
			opts.WithApplicationType("web"));

		var grant = await module.AuthorizeAsync(opts => opts.WithPkce(null));

		var response = await module.RequestTokenAsync(opts =>
		{
			opts.WithGrantType("authorization_code");
			opts.WithCode(grant.Code);
			opts.WithClientId(module.Id);
			opts.WithRedirectUri(DefaultValues.RedirectUri);
			opts.WithAudience(DefaultValues.ApiAudience);
			opts.WithScopes(DefaultValues.Scopes);
			opts.WithClientSecret(module.Secret);
		});

		Assert.NotNull(response);
		Assert.NotNull(response.AccessToken);
		Assert.Null(response.Error);
	}
    
	[Fact]
	public async Task AuhtorizationCode_WhenInvalidRedirectUri_ReturnsInvalidGrantError()
	{
		var module = await _host.CreateApiClientAsync(opts =>
			opts.WithApplicationType("web"));

		var grant = await module.AuthorizeAsync();

		var response = await module.RequestTokenAsync(opts =>
		{
			opts.WithGrantType("authorization_code");
			opts.WithCode(grant.Code);
			opts.WithClientId(module.Id);
			opts.WithRedirectUri("https://invalid-uri.com");
			opts.WithAudience(DefaultValues.ApiAudience);
			opts.WithScopes(DefaultValues.Scopes);
			opts.WithClientSecret(module.Secret);
		});

		Assert.NotNull(response);
		Assert.Equal((string?)"invalid_grant", (string?)response.Error);
		Assert.Contains((string)"redirect_uri", (string?)response.ErrorDescription);
	}

	[Fact]
	public async Task AuthorizationCodePkce_Success()
	{
		var module = await _host.CreateApiClientAsync(opts =>
			opts.WithApplicationType("spa"));

		var (verifier, pkce) = PkceHelpers.Create();

		var grant = await module.AuthorizeAsync();

		var response = await module.RequestTokenAsync(opts =>
		{
			opts.WithGrantType("authorization_code");
			opts.WithCode(grant.Code);
			opts.WithClientId(module.Id);
			opts.WithRedirectUri(DefaultValues.RedirectUri);
			opts.WithAudience(DefaultValues.ApiAudience);
			opts.WithScopes(DefaultValues.Scopes);
			opts.WithCodeVerifier(verifier);
		});

		Assert.NotNull(response);
		Assert.NotNull(response.AccessToken);
		Assert.Null(response.Error);
	}
    
	[Fact]
	public async Task AuthorizationCodePkce_WithInvalidVerifier_ReturnsInvalidGrantError()
	{
		var module = await _host.CreateApiClientAsync(opts =>
			opts.WithApplicationType("spa"));

		var grant = await module.AuthorizeAsync();

		var response = await module.RequestTokenAsync(opts =>
		{
			opts.WithGrantType("authorization_code");
			opts.WithCode(grant.Code);
			opts.WithClientId(module.Id);
			opts.WithRedirectUri(DefaultValues.RedirectUri);
			opts.WithAudience(DefaultValues.ApiAudience);
			opts.WithScopes(DefaultValues.Scopes);
			opts.WithCodeVerifier("invalid-code-verifier");
		});

		Assert.NotNull(response);
		Assert.Equal((string?)"invalid_grant", (string?)response.Error);
		Assert.Contains((string)"code verifier", (string?)response.ErrorDescription);
	}
    
	[Fact]
	public async Task AuthorizationCodePkce_WithOidc_Success()
	{
		await using var scope = _host.CreateScope();
		await Given.UserAsync(scope);
		
		var client = await _host.CreateApiClientAsync(opts =>
			opts.WithApplicationType("spa"));

		var (verifier, pkce) = PkceHelpers.Create();

		const string scopes = "openid profile";
		var grant = await client.AuthorizeAsync(opts =>
		{
			opts.WithPkce(pkce);
			opts.WithScope(scopes);
			opts.WithNonce("test-nonce");
		});

		var response = await client.RequestTokenAsync(opts =>
		{
			opts.WithGrantType("authorization_code");
			opts.WithCode(grant.Code);
			opts.WithClientId(client.Id);
			opts.WithRedirectUri(DefaultValues.RedirectUri);
			opts.WithAudience(DefaultValues.ApiAudience);
			opts.WithScopes(scopes);
			opts.WithCodeVerifier(verifier);
		});

		Assert.NotNull(response);
		Assert.NotNull(response.AccessToken);
		Assert.NotNull(response.IdToken);
		Assert.Null(response.Error);
	}
}