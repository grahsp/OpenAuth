using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Test.Unit.OAuth.Jwt;

public class JwtBuilderTests
{
    private const string Issuer = "https://example.com/";
    
    private readonly FakeTimeProvider _time = new();

    private JwtBuilder CreateValidBuilder()
        => new JwtBuilder(Issuer)
            .WithClient(ClientId.New())
            .WithSubject("subject")
            .WithAudiences(new AudienceName("api"));


    
    [Fact]
    public void Build_WhenValid_AddsSystemClaims()
    {
        var descriptor = CreateValidBuilder()
            .Build(_time);
        
        var iss = Assert.Single(descriptor.Claims, c => c.Type == OAuthClaimTypes.Iss);
        var iat = Assert.Single(descriptor.Claims, c => c.Type == OAuthClaimTypes.Iat);
        var jti = Assert.Single(descriptor.Claims, c => c.Type == OAuthClaimTypes.Jti);

        Assert.Equal(Issuer, iss.Value);
        Assert.Equal(_time.GetUtcNow().ToUnixTimeSeconds().ToString(), iat.Value);
        Assert.NotNull(jti.Value);
    }

    [Fact]
    public void Build_WhenValid_AddsExplicitClaims()
    {
        var clientId = ClientId.New();
        var subject = "subject";
        var audience = new AudienceName("api");
        
        var descriptor = new JwtBuilder(Issuer)
            .WithClient(clientId)
            .WithSubject(subject)
            .WithAudiences(audience)
            .Build(_time);
        
        var actualClientId = Assert.Single(descriptor.Claims, c => c.Type == OAuthClaimTypes.ClientId);
        var actualSubject = Assert.Single(descriptor.Claims, c => c.Type == OAuthClaimTypes.Sub);
        var actualAudience = Assert.Single(descriptor.Claims, c => c.Type == OAuthClaimTypes.Aud);
        
        Assert.Equal(clientId.ToString(), actualClientId.Value);
        Assert.Equal(subject, actualSubject.Value);
        Assert.Equal(audience.Value, actualAudience.Value);
    }

    [Fact]
    public void Build_WhenValid_SetsDescriptorMetadata()
    {
        var now = _time.GetUtcNow();
        var lifetime = TimeSpan.FromMinutes(10);
        
        var descriptor = CreateValidBuilder()
            .WithLifetime(lifetime)
            .Build(_time);
        
        Assert.Equal(Issuer, descriptor.Issuer);
        Assert.Equal(now, descriptor.IssuedAt);
        Assert.Equal(now.Add(lifetime), descriptor.ExpiresAt);
        Assert.Equal(now, descriptor.NotBefore);
        Assert.Equal(lifetime, descriptor.Lifetime);
    }

    [Fact]
    public void Build_WithoutClientId_ThrowsException()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => new JwtBuilder(Issuer)
                .WithAudiences(new AudienceName("api"))
                .WithSubject("subject")
                .Build(_time));
        
        Assert.Contains("client id", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_WithoutAudience_ThrowsException()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => new JwtBuilder(Issuer)
                .WithClient(ClientId.New())
                .WithSubject("subject")
                .Build(_time));

        Assert.Contains("audience", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public void Build_WithoutSubject_ThrowsException()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => new JwtBuilder(Issuer)
                .WithClient(ClientId.New())
                .WithAudiences(new AudienceName("api"))
                .Build(_time));
        
        Assert.Contains("subject", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}