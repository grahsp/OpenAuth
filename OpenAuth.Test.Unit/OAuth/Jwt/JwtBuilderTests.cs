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


    public class WithSubject : JwtBuilderTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void WithSubject_WhenInvalid_ThrowException(string? subject)
        {
            var builder = new JwtBuilder(Issuer);
            
            Assert.ThrowsAny<ArgumentException>(()
                => builder.WithSubject(subject!));
        }

        [Fact]
        public void WithSubject_WhenSubjectAlreadySet_ThrowsException()
        {
            var builder = new JwtBuilder(Issuer)
                .WithSubject("subject");
            
            Assert.Throws<InvalidOperationException>(()
                => builder.WithSubject("subject"));
        }

        [Fact]
        public void WithSubject_AfterWithClaims_ThrowsException()
        {
            var builder = new JwtBuilder(Issuer)
                .WithClaim(OAuthClaimTypes.Sub, "subject");

            Assert.Throws<InvalidOperationException>(()
                => builder.WithSubject("subject"));
        }
        
        [Fact]
        public void WithSubject_WhenValid_AddsSubject()
        {
            var descriptor = new JwtBuilder(Issuer)
                .WithClient(ClientId.New())
                .WithSubject("subject")
                .WithAudience(new AudienceName("api"))
                .Build(_time);
            
            var sub = Assert.Single(descriptor.Claims, c => c.Type == OAuthClaimTypes.Sub);
            Assert.Equal("subject", sub.Value);
        }
    }

    public class WithAudience : JwtBuilderTests
    {
        [Fact]
        public void WithAudience_AudienceNull_ThrowsException()
        {
            var builder = new JwtBuilder(Issuer);
            
            Assert.ThrowsAny<ArgumentException>(()
                => builder.WithAudience(null!));
        }

        [Fact]
        public void WithAudience_WhenAudienceAlreadySet_ThrowsException()
        {
            var builder = new JwtBuilder(Issuer)
                .WithAudience(new AudienceName("web"));
            
            Assert.Throws<InvalidOperationException>(()
                => builder.WithAudience(new AudienceName("api")));
        }

        [Fact]
        public void WithAudience_AfterWithClaims_ThrowsException()
        {
            var builder = new JwtBuilder(Issuer)
                .WithClaim(OAuthClaimTypes.Aud, "audience");
            
            Assert.Throws<InvalidOperationException>(()
                => builder.WithAudience(new AudienceName("api")));
        }
        
        [Fact]
        public void WithAudience_WhenValid_AddsAudience()
        {
            var audience = new AudienceName("api");
            
            var descriptor = new JwtBuilder(Issuer)
                .WithClient(ClientId.New())
                .WithSubject("subject")
                .WithAudience(audience)
                .Build(_time);
            
            var aud = Assert.Single(descriptor.Claims, c => c.Type == OAuthClaimTypes.Aud);
            Assert.Equal(audience.Value, aud.Value);
        }
    }

    public class WithScopes : JwtBuilderTests
    {
        [Fact]
        public void WithScopes_WhenScopeIsNull_ThrowsException()
        {
            Assert.ThrowsAny<ArgumentException>(()
                => CreateValidBuilder()
                    .WithScopes(new Scope("read"), null!));
        }

        [Fact]
        public void WithScopes_WhenEmpty_AddsNoScopes()
        {
            var descriptor = CreateValidBuilder()
                .WithScopes()
                .Build(_time);
            
            var scopes = descriptor.Claims
                .Where(c => c.Type == OAuthClaimTypes.Scope)
                .ToArray();
            
            Assert.Empty(scopes);
        }

        [Fact]
        public void WithScopes_SingleScope_AddsScope()
        {
            var read = new Scope("read");
            
            var descriptor = CreateValidBuilder()
                .WithScopes(read)
                .Build(_time);
            
            var scope = Assert.Single(descriptor.Claims, s => s.Type == OAuthClaimTypes.Scope);
            Assert.Equal(read.Value, scope.Value);
        }

        [Fact]
        public void WithScopes_MultipleScopes_AddsScopes()
        {
            var read = new Scope("read");
            var write = new Scope("write");
            
            var descriptor = CreateValidBuilder()
                .WithScopes(read, write)
                .Build(_time);
            
            var scopes = descriptor.Claims
                .Where(c => c.Type == OAuthClaimTypes.Scope)
                .ToArray();
            
            Assert.Equal(2, scopes.Length);
            Assert.Contains(scopes, c => c.Value == read.Value);
            Assert.Contains(scopes, c => c.Value == write.Value);
        }
        
        [Fact]
        public void WithScopes_CalledMultipleTimes_AddsAllScopes()
        {
            var read = new Scope("read");
            var write = new Scope("write");
            
            var descriptor = CreateValidBuilder()
                .WithScopes(read)
                .WithScopes(write)
                .Build(_time);
            
            var scopes = descriptor.Claims
                .Where(c => c.Type == OAuthClaimTypes.Scope)
                .ToArray();
            
            Assert.Equal(2, scopes.Length);
            Assert.Contains(scopes, c => c.Value == read.Value);
            Assert.Contains(scopes, c => c.Value == write.Value);
        }
    }

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
            Assert.ThrowsAny<ArgumentException>(() => CreateValidBuilder()
                .WithClaim(type!, "value"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void WithClaim_InvalidClaimValue_ThrowsException(string? value)
        {
            Assert.ThrowsAny<ArgumentException>(() => CreateValidBuilder()
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