using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Security.Tokens;
using OpenAuth.Test.Common.Fakes;

namespace OpenAuth.Test.Unit.Security.Tokens;

public class JwtTokenGeneratorTests
{
    private static SigningKey CreateSigningKey() =>
        CreateSigningKey(null);
    
    private static SigningKey CreateSigningKey(DateTime? expires) =>
        SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)), expires);

    private static Client CreateClientWithScopes(string name, Audience audience, params Scope[] scopes)
    {
        var client = new Client(name);
        client.TryAddAudience(audience);
        client.GrantScopes(audience, scopes);
        return client;
    }

    private static JwtTokenGenerator CreateSut()
    {
        return new JwtTokenGenerator(Options.Create("test-issuer"), new FakeSigningCredentialsFactory());
    }

    [Fact]
    public void GenerateToken_IncludesExpectedClaims()
    {
        var sut = CreateSut();
        var key = CreateSigningKey();
        var audience = new Audience("api");
        var scope = new Scope("read");
        var client = CreateClientWithScopes("client123", audience, scope);

        var token = sut.GenerateToken(client, audience, [scope], key);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal((string?)"test-issuer", (string?)jwt.Issuer);
        Assert.Equal((string?)client.Id.ToString(), (string?)jwt.Subject);
        Assert.Contains(audience.Value, jwt.Audiences);
        Assert.Contains(jwt.Claims, c => c.Type == "scope" && c.Value == scope.Value);
    }

    [Fact]
    public void GenerateToken_Throws_WhenSigningKeyInactive()
    {
        var sut = CreateSut();
        var key = CreateSigningKey(DateTime.UtcNow.AddSeconds(-1));
        var audience = new Audience("api");
        var scope = new Scope("read");
        var client = CreateClientWithScopes("client123", audience, scope);

        Assert.Throws<InvalidOperationException>(() =>
            sut.GenerateToken(client, audience, [scope], key));
    }

    [Fact]
    public void GenerateToken_Throws_WhenAudienceNotAllowed()
    {
        var sut = CreateSut();
        var key = CreateSigningKey();
        var client = new Client("client123");

        Assert.Throws<InvalidOperationException>(() =>
            sut.GenerateToken(client, new Audience("api"), [new Scope("read")], key));
    }

    [Fact]
    public void GenerateToken_Throws_WhenInvalidScopeRequested()
    {
        var sut = CreateSut();
        var key = CreateSigningKey();
        var audience = new Audience("api");
        var client = CreateClientWithScopes("client123", audience, new Scope("read"));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            sut.GenerateToken(client, audience, [new Scope("write")], key));

        Assert.Contains("Invalid scopes", ex.Message);
    }
}