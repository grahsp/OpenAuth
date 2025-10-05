using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Security.Tokens;
using OpenAuth.Domain.Configurations;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Security.Tokens;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Fakes;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.Security.Tokens;

public class JwtTokenGeneratorTests
{
    private FakeTimeProvider _time;
    private IJwtTokenGenerator _sut;
    private SigningKey _signingKey;

    public JwtTokenGeneratorTests()
    {
        _time = new FakeTimeProvider();
        _sut = new JwtTokenGenerator(
            Options.Create(new Auth { Issuer = "test-issuer" }),
            new FakeHmacSigningCredentialsFactory(),
            _time);
        
        _signingKey = TestSigningKey.CreateHmacSigningKey(_time);
    }
    

    [Fact]
    public void GenerateToken_WithValidInputs_ReturnsTokenWithExpectedClaims()
    {
        // Arrange
        var client = new ClientBuilder().Build();
        
        var audienceName = new AudienceName("api");
        var scopes = new[] { new Scope("read"), new Scope("write") };
        
        var audience = client.AddAudience(audienceName, _time.GetUtcNow());
        client.GrantScopes(audienceName, scopes, _time.GetUtcNow());

        // Act
        var token = _sut.GenerateToken(client, audience, audience.Scopes, _signingKey);

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        
        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Equal(client.Id.Value.ToString(), jwt.Subject);
        Assert.Contains(audienceName.Value, jwt.Audiences);

        var scopeClaims = jwt.Claims
            .Where(c => c.Type == "scope")
            .Select(c => c.Value)
            .OrderBy(s => s)
            .ToArray();
        
        Assert.Equal(["read", "write"], scopeClaims);
    }

     [Fact]
    public void GenerateToken_WithInactiveSigningKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var now = _time.GetUtcNow();
        var expiredKey = TestSigningKey.CreateHmacSigningKey(
            now.DateTime, 
            now.DateTime.AddSeconds(-1)); // Already expired
        
        var client = new ClientBuilder().Build();
        var audienceName = new AudienceName("api");
        var audience = client.AddAudience(audienceName, _time.GetUtcNow());
        client.GrantScopes(audienceName, [new Scope("read")], _time.GetUtcNow());

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _sut.GenerateToken(client, audience, audience.Scopes, expiredKey));
        
        Assert.Contains("expired", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateToken_WithAudienceNotBelongingToClient_ThrowsInvalidOperationException()
    {
        // Arrange
        var client = new ClientBuilder().Build();
        
        // Create audience that doesn't belong to this client
        var otherClient = new ClientBuilder().Build();
        var otherAudienceName = new AudienceName("api");
        var otherAudience = otherClient.AddAudience(otherAudienceName, _time.GetUtcNow());

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _sut.GenerateToken(client, otherAudience, [new Scope("read")], _signingKey));
        
        Assert.Contains("Audience doesn't exist.", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateToken_WithScopesNotGrantedToAudience_ThrowsInvalidOperationException()
    {
        // Arrange
        var client = new ClientBuilder().Build();
        var audienceName = new AudienceName("api");
        var audience = client.AddAudience(audienceName, _time.GetUtcNow());
        
        // Only grant 'read' scope
        client.GrantScopes(audienceName, [new Scope("read")], _time.GetUtcNow());
        
        // Try to generate token with 'write' scope that wasn't granted
        var requestedScopes = new[] { new Scope("write") };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _sut.GenerateToken(client, audience, requestedScopes, _signingKey));

        Assert.Contains("Invalid scopes", exception.Message);
    }

    [Fact]
    public void GenerateToken_WithSubsetOfGrantedScopes_ReturnsTokenWithRequestedScopesOnly()
    {
        // Arrange
        var client = new ClientBuilder().Build();
        var audienceName = new AudienceName("api");
        var audience = client.AddAudience(audienceName, _time.GetUtcNow());
        
        // Grant multiple scopes
        var grantedScopes = new[] { new Scope("read"), new Scope("write"), new Scope("delete") };
        client.GrantScopes(audienceName, grantedScopes, _time.GetUtcNow());
        
        // Request only subset
        var requestedScopes = new[] { new Scope("read"), new Scope("write") };

        // Act
        var token = _sut.GenerateToken(client, audience, requestedScopes, _signingKey);

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var scopeClaims = jwt.Claims
            .Where(c => c.Type == "scope")
            .Select(c => c.Value)
            .OrderBy(s => s)
            .ToArray();
        
        Assert.Equal(["read", "write"], scopeClaims);
        Assert.DoesNotContain("delete", scopeClaims);
    }

    [Fact]
    public void GenerateToken_WithEmptyScopes_ReturnsTokenWithNoScopeClaims()
    {
        // Arrange
        var client = new ClientBuilder().Build();
        var audienceName = new AudienceName("api");
        var audience = client.AddAudience(audienceName, _time.GetUtcNow());
        
        // No scopes granted

        // Act
        var token = _sut.GenerateToken(client, audience, [], _signingKey);

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var scopeClaims = jwt.Claims.Where(c => c.Type == "scope");
        
        Assert.Empty(scopeClaims);
    }

    [Fact]
    public void GenerateToken_WithMultipleAudiences_IncludesCorrectAudienceInToken()
    {
        // Arrange
        var client = new ClientBuilder().Build();
        
        var apiAudienceName = new AudienceName("api");
        var webAudienceName = new AudienceName("web");
        
        var apiAudience = client.AddAudience(apiAudienceName, _time.GetUtcNow());
        client.AddAudience(webAudienceName, _time.GetUtcNow());
        
        client.GrantScopes(apiAudienceName, [new Scope("read")], _time.GetUtcNow());
        client.GrantScopes(webAudienceName, [new Scope("admin")], _time.GetUtcNow());

        // Act - generate token for API audience only
        var token = _sut.GenerateToken(client, apiAudience, apiAudience.Scopes, _signingKey);

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        
        Assert.Contains(apiAudienceName.Value, jwt.Audiences);
        Assert.DoesNotContain(webAudienceName.Value, jwt.Audiences);
        
        var scopeClaims = jwt.Claims
            .Where(c => c.Type == "scope").Select(c => c.Value)
            .ToArray();
        
        Assert.Contains("read", scopeClaims);
        Assert.DoesNotContain("admin", scopeClaims);
    }

    [Fact]
    public void GenerateToken_SetsExpirationBasedOnClientTokenLifetime()
    {
        // Arrange
        var client = new ClientBuilder().Build();
        var audienceName = new AudienceName("api");
        var audience = client.AddAudience(audienceName, _time.GetUtcNow());
        client.GrantScopes(audienceName, [new Scope("read")], _time.GetUtcNow());

        // Act
        var token = _sut.GenerateToken(client, audience, audience.Scopes, _signingKey);

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var expectedExpiration = _time.GetUtcNow().Add(client.TokenLifetime);
        
        Assert.Equal(expectedExpiration.DateTime, jwt.ValidTo, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GenerateToken_SetsIssuedAtToCurrentTime()
    {
        // Arrange
        var client = new ClientBuilder()
            .CreatedAt(_time.GetUtcNow())
            .Build();
        
        var audienceName = new AudienceName("api");
        var audience = client.AddAudience(audienceName, _time.GetUtcNow());
        client.GrantScopes(audienceName, [new Scope("read")], _time.GetUtcNow());

        // Act
        var token = _sut.GenerateToken(client, audience, audience.Scopes, _signingKey);

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        
        Assert.Equal(_time.GetUtcNow().DateTime, jwt.IssuedAt, precision: TimeSpan.FromSeconds(1));
        Assert.Equal(_time.GetUtcNow().DateTime, jwt.ValidFrom, precision: TimeSpan.FromSeconds(1));
    }
}