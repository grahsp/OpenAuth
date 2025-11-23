using System.Security.Claims;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Tokens.Builders;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Test.Unit.OAuth.Jwt;

public class JwtBuilderTests
{
    private const string Issuer = "https://oauth-provider.com/";
    private readonly FakeTimeProvider _time = new();
    
    [Fact]
    public void WithClaim_AddsRequiredClaim()
    {
        var builder = new JwtBuilder(Issuer, _time);

        builder.AddClaim("custom", "value");

        var descriptor = builder.Build();

        Assert.Contains(descriptor.Claims, c => c is { Type: "custom", Value: "value" });
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithOptionalClaim_DoesNotAdd_WhenValueIsNullOrWhitespace(string? value)
    {
        var builder = new JwtBuilder(Issuer, _time);

        builder.AddOptionalClaim("x", value);

        var descriptor = builder.Build();

        Assert.DoesNotContain(descriptor.Claims, c => c.Type == "x");
    }
 
    [Fact]
    public void Build_AppliesDefaultLife_time_WhenNoneProvided()
    {
        var utcNow = _time.GetUtcNow();
        var builder = new JwtBuilder(Issuer, _time);

        var descriptor = builder.Build();

        Assert.Equal(TimeSpan.FromMinutes(30), descriptor.Lifetime);
        Assert.Equal(utcNow.AddMinutes(30), descriptor.ExpiresAt);
    }
    
    [Fact]
    public void WithLife_time_OverridesDefault()
    {
        var utcNow = _time.GetUtcNow();
        var builder = new JwtBuilder(Issuer, _time)
            .WithLifetime(TimeSpan.FromHours(1));

        var descriptor = builder.Build();

        Assert.Equal(TimeSpan.FromHours(1), descriptor.Lifetime);
        Assert.Equal(utcNow.AddHours(1), descriptor.ExpiresAt);
    }

    [Fact]
    public void Build_AddsRequiredStandardClaims()
    {
        var builder = new JwtBuilder(Issuer, _time);

        var descriptor = builder.Build();

        Assert.Contains(descriptor.Claims, c => c.Type == OAuthClaimTypes.Iss);
        Assert.Contains(descriptor.Claims, c => c.Type == OAuthClaimTypes.Jti);
        Assert.Contains(descriptor.Claims, c => c.Type == OAuthClaimTypes.Iat);
        Assert.Contains(descriptor.Claims, c => c.Type == OAuthClaimTypes.Exp);
        Assert.Contains(descriptor.Claims, c => c.Type == OAuthClaimTypes.Nbf);
    }
    
    [Fact]
    public void Build_WhenReservedClaimDuplicated_Throws()
    {
        var builder = new JwtBuilder(Issuer, _time)
            .AddClaim(OAuthClaimTypes.Iss, "something_else");

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void WithClaims_AddsMultipleClaims()
    {
        var builder = new JwtBuilder(Issuer, _time);

        var claims = new[]
        {
            new Claim("c1", "v1"),
            new Claim("c2", "v2")
        };

        builder.AddClaims(claims);

        var descriptor = builder.Build();

        Assert.Contains(descriptor.Claims, c => c is { Type: "c1", Value: "v1" });
        Assert.Contains(descriptor.Claims, c => c is { Type: "c2", Value: "v2" });
    }
}