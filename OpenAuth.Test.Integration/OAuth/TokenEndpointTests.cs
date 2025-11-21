using OpenAuth.Test.Common.Helpers;
using OpenAuth.Test.Integration.Infrastructure.Fixtures;

namespace OpenAuth.Test.Integration.OAuth;

[Collection("sqlserver")]
public class TokenEndpointTests(ApiServerFixture fx) : IClassFixture<ApiServerFixture>, IAsyncLifetime
{
    public Task InitializeAsync() => fx.InitializeAsync();
    public Task DisposeAsync() => Task.CompletedTask;


    [Fact]
    public async Task ClientCredentials_Success()
    {
        var client = await fx.CreateClientAsync(opts =>
            opts.WithApplicationType("m2m"));

        var response = await client.RequestTokenAsync(opts =>
        {
            opts.WithGrantType("client_credentials");
            opts.WithAudience(DefaultValues.Audience);
            opts.WithScopes(DefaultValues.Scopes);
        });

        Assert.NotNull(response);
        Assert.NotNull(response.Token);
    }

    [Fact]
    public async Task AuthorizationCode_Success()
    {
        var client = await fx.CreateClientAsync(opts =>
            opts.WithApplicationType("web"));

        var grant = await client.AuthorizeAsync();

        var result = await client.RequestTokenAsync(opts =>
        {
            opts.WithGrantType("authorization_code");
            opts.WithCode(grant.Code);
            opts.WithClientId(client.Id);
            opts.WithRedirectUri(DefaultValues.RedirectUri);
            opts.WithAudience(DefaultValues.Audience);
            opts.WithScopes(DefaultValues.Scopes);
            opts.WithClientSecret(client.Secret);
        });

        Assert.NotNull(result);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task AuthorizationCodePkce_Success()
    {
        var client = await fx.CreateClientAsync(opts =>
            opts.WithApplicationType("spa"));

        var (verifier, pkce) = PkceHelpers.Create();

        var grant = await client.AuthorizeAsync(opts =>
            opts.WithPkce(pkce.CodeChallenge, pkce.CodeChallengeMethod.ToString()));

        var result = await client.RequestTokenAsync(opts =>
        {
            opts.WithGrantType("authorization_code");
            opts.WithCode(grant.Code);
            opts.WithClientId(client.Id);
            opts.WithRedirectUri(DefaultValues.RedirectUri);
            opts.WithAudience(DefaultValues.Audience);
            opts.WithScopes(DefaultValues.Scopes);
            opts.WithCodeVerifier(verifier);
        });

        Assert.NotNull(result);
        Assert.NotNull(result.Token);       
    }
}