using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Configurations;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Security.Tokens;
using OpenAuth.Test.Common.Fakes;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.Security.Tokens;

public class JwtTokenGeneratorTests
{
    private readonly FakeTimeProvider _time = new();
    
    private Client CreateClientWithScopes(string name, Audience audience, params Scope[] scopes)
    {
        var now = _time.GetUtcNow();
        var client = new Client(new ClientName(name), now);
        
        client.TryAddAudience(audience, now);
        client.GrantScopes(audience, scopes, now);
        
        return client;
    }

    private JwtTokenGenerator CreateSut()
        => new JwtTokenGenerator(
            Options.Create(new Auth { Issuer = "test-issuer" }),
            new FakeHmacSigningCredentialsFactory(),
            _time
        );

    [Fact]
    public void GenerateToken_IncludesExpectedClaims()
    {
        var sut = CreateSut();
        var key = TestSigningKey.CreateHmacSigningKey(_time);
        var audience = new Audience("api");
        var scope = new Scope("read");
        var client = CreateClientWithScopes("client123", audience, scope);

        var token = sut.GenerateToken(client, audience, [scope], key);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Equal(client.Id.ToString(), jwt.Subject);
        Assert.Contains(audience.Value, jwt.Audiences);
        Assert.Contains(jwt.Claims, c => c.Type == "scope" && c.Value == scope.Value);
    }

    [Fact]
    public void GenerateToken_Throws_WhenSigningKeyInactive()
    {
        var sut = CreateSut();
        var key = TestSigningKey.CreateRsaSigningKey(_time.GetUtcNow().DateTime, _time.GetUtcNow().DateTime.AddSeconds(-1));
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
        var key = TestSigningKey.CreateRsaSigningKey(_time);
        var client = new Client(new ClientName("test"), _time.GetUtcNow());

        Assert.Throws<InvalidOperationException>(() =>
            sut.GenerateToken(client, new Audience("api"), [new Scope("read")], key));
    }

    [Fact]
    public void GenerateToken_Throws_WhenInvalidScopeRequested()
    {
        var sut = CreateSut();
        var key = TestSigningKey.CreateHmacSigningKey(_time);
        var audience = new Audience("api");
        var client = CreateClientWithScopes("client123", audience, new Scope("read"));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            sut.GenerateToken(client, audience, [new Scope("write")], key));

        Assert.Contains("Invalid scopes", ex.Message);
    }
}