using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.Tokens;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Interfaces;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Infrastructure.Configurations;
using OpenAuth.Infrastructure.Tokens;

namespace OpenAuth.Test.Unit.Security.Tokens;

public class JwtTokenGeneratorTests
{
    private readonly FakeTimeProvider _time;
    private readonly ISigningCredentialsFactory _credentialsFactory;
    private readonly IJwtTokenGenerator _sut;
    private readonly SigningKeyData _keyData;

    public JwtTokenGeneratorTests()
    {
        _time = new FakeTimeProvider();
        _credentialsFactory = Substitute.For<ISigningCredentialsFactory>();
        
        _sut = new JwtTokenGenerator(
            Options.Create(new JwtOptions { Issuer = "test-issuer" }),
            _credentialsFactory,
            _time);
        
        _keyData = new SigningKeyData(
            SigningKeyId.New(),
            KeyType.RSA,
            SigningAlgorithm.RS256,
            new Key("test-private-key")
        );
        
        // Setup mock to return valid signing credentials
        _credentialsFactory.Create(Arg.Any<SigningKeyData>())
            .Returns(info => {
                var keyData = info.Arg<SigningKeyData>();
                return TestSigningCredentials.CreateRsa(keyData.Kid.Value.ToString());
            });
    }

    public class GenerateToken : JwtTokenGeneratorTests
    {
        [Fact]
        public void ReturnsTokenWithExpectedClaims()
        {
            // Arrange
            var clientId = ClientId.New();
            var audience = new AudienceName("api");
            var scopes = new[] { new Scope("read"), new Scope("write") };
            var lifetime = TimeSpan.FromMinutes(10);
            
            var request = new TokenGenerationRequest(
                clientId,
                audience,
                scopes,
                lifetime,
                _keyData
            );

            // Act
            var token = _sut.GenerateToken(request);

            // Assert
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            
            Assert.Equal("test-issuer", jwt.Issuer);
            Assert.Equal(clientId.Value.ToString(), jwt.Subject);
            Assert.Contains(audience.Value, jwt.Audiences);

            var scopeClaims = jwt.Claims
                .Where(c => c.Type == "scope")
                .Select(c => c.Value)
                .OrderBy(s => s)
                .ToArray();
            
            Assert.Equal(["read", "write"], scopeClaims);
        }

        [Fact]
        public void SetsExpirationBasedOnTokenLifetime()
        {
            // Arrange
            var now = _time.GetUtcNow();
            var lifetime = TimeSpan.FromHours(2);
            
            var request = new TokenGenerationRequest(
                ClientId.New(),
                new AudienceName("api"),
                [new Scope("read")],
                lifetime,
                _keyData
            );

            // Act
            var token = _sut.GenerateToken(request);

            // Assert
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var expectedExpiration = now.Add(lifetime);
            
            Assert.Equal(expectedExpiration.DateTime, jwt.ValidTo, precision: TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void SetsIssuedAtToCurrentTime()
        {
            // Arrange
            var now = _time.GetUtcNow();
            
            var request = new TokenGenerationRequest(
                ClientId.New(),
                new AudienceName("api"),
                [new Scope("read")],
                TimeSpan.FromHours(1),
                _keyData
            );

            // Act
            var token = _sut.GenerateToken(request);

            // Assert
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            
            Assert.Equal(now.DateTime, jwt.IssuedAt, precision: TimeSpan.FromSeconds(1));
            Assert.Equal(now.DateTime, jwt.ValidFrom, precision: TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void IncludesJtiClaim()
        {
            // Arrange
            var request = new TokenGenerationRequest(
                ClientId.New(),
                new AudienceName("api"),
                [new Scope("read")],
                TimeSpan.FromHours(1),
                _keyData
            );

            // Act
            var token = _sut.GenerateToken(request);

            // Assert
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
            
            Assert.NotNull(jti);
            Assert.NotEmpty(jti.Value);
            Assert.True(Guid.TryParse(jti.Value, out _));
        }

        [Fact]
        public void WithEmptyScopes_ReturnsTokenWithNoScopeClaims()
        {
            // Arrange
            var request = new TokenGenerationRequest(
                ClientId.New(),
                new AudienceName("api"),
                [],
                TimeSpan.FromHours(1),
                _keyData
            );

            // Act
            var token = _sut.GenerateToken(request);

            // Assert
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var scopeClaims = jwt.Claims.Where(c => c.Type == "scope");
            
            Assert.Empty(scopeClaims);
        }

        [Fact]
        public void WithMultipleScopes_IncludesAllInToken()
        {
            // Arrange
            var scopes = new[] 
            { 
                new Scope("read"), 
                new Scope("write"), 
                new Scope("delete") 
            };
            
            var request = new TokenGenerationRequest(
                ClientId.New(),
                new AudienceName("api"),
                scopes,
                TimeSpan.FromHours(1),
                _keyData
            );

            // Act
            var token = _sut.GenerateToken(request);

            // Assert
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var scopeClaims = jwt.Claims
                .Where(c => c.Type == "scope")
                .Select(c => c.Value)
                .OrderBy(s => s)
                .ToArray();
            
            Assert.Equal(["delete", "read", "write"], scopeClaims);
        }

        [Fact]
        public void CallsSigningCredentialsFactoryWithKeyData()
        {
            // Arrange
            var request = new TokenGenerationRequest(
                ClientId.New(),
                new AudienceName("api"),
                [new Scope("read")],
                TimeSpan.FromHours(1),
                _keyData
            );

            // Act
            _sut.GenerateToken(request);

            // Assert
            _credentialsFactory.Received(1).Create(_keyData);
        }

        [Fact]
        public void IncludesKidInHeader()
        {
            // Arrange
            var request = new TokenGenerationRequest(
                ClientId.New(),
                new AudienceName("api"),
                [new Scope("read")],
                TimeSpan.FromHours(1),
                _keyData
            );

            // Act
            var token = _sut.GenerateToken(request);

            // Assert
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.True(jwt.Header.ContainsKey("kid"));
            Assert.Equal(_keyData.Kid.Value.ToString(), jwt.Header["kid"]);
        }
    }
}

// Test helper for creating signing credentials
public static class TestSigningCredentials
{
    public static SigningCredentials CreateRsa(string kid)
    {
        var rsa = RSA.Create(2048);
        var key = new RsaSecurityKey(rsa) { KeyId = kid };
        return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
    }
}