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
            .WithAudience(new AudienceName("api"));


    public class WithClaim : JwtBuilderTests
    {
        [Fact]
        public void WithClaim_WhenValid_AddsClaim()
        {
            const string type = "type";
            const string value = "value";
        
            var descriptor = CreateValidBuilder()
                .WithClaim(type, value)
                .Build(_time);
        
            var claim = Assert.Single(descriptor.Claims, c => c.Type == type);
            Assert.Equal(value, claim.Value);
        }

        [Fact]
        public void WithClaim_SameClaimType_AddsAllClaims()
        {
            const string type = "role";
        
            var descriptor = CreateValidBuilder()
                .WithClaim(type, "user")
                .WithClaim(type, "editor")
                .WithClaim(type, "admin")
                .Build(_time);
        
            var roles = descriptor.Claims
                .Where(c => c.Type == type)
                .ToArray();
        
            Assert.Contains(roles, c => c.Value == "user");
            Assert.Contains(roles, c => c.Value == "editor");
            Assert.Contains(roles, c => c.Value == "admin");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void WithClaim_InvalidClaimType_ThrowsException(string? type)
        {
            Assert.ThrowsAny<ArgumentException>(() => new JwtBuilder(Issuer)
                .WithClaim(type!, "value"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void WithClaim_InvalidClaimValue_ThrowsException(string? value)
        {
            Assert.ThrowsAny<ArgumentException>(() => new JwtBuilder(Issuer)
                .WithClaim("type", value!));
        }
    }

    public class Build : JwtBuilderTests
    {
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
                .WithAudience(audience)
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
                    .WithAudience(new AudienceName("api"))
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
                    .WithAudience(new AudienceName("api"))
                    .Build(_time));
        
            Assert.Contains("subject", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}