using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Infrastructure.Configurations;
using OpenAuth.Infrastructure.Tokens;
using TokenContext = OpenAuth.Application.Tokens.Dtos.TokenContext;

namespace OpenAuth.Test.Unit.Clients.Infrastructure;

public class JwtTokenGeneratorTests
{
    private readonly FakeTimeProvider _time;
    private readonly ISigningCredentialsFactory _credentialsFactory;
    private readonly JwtTokenGenerator _sut;
    private readonly SigningKeyData _keyData;
    private readonly TokenContext _defaultContext;
    private readonly ClientTokenData _defaultTokenData;

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
            new Key("test-private-key"));

        _credentialsFactory.Create(Arg.Any<SigningKeyData>())
            .Returns(callInfo =>
            {
                var keyData = callInfo.Arg<SigningKeyData>();
                return TestSigningCredentials.CreateRsa(keyData.Kid.Value.ToString());
            });

        _defaultContext = new TokenContext(
            ClientId.New(),
            "test-user",
            new AudienceName("api"),
            ScopeCollection.Parse("read write")
        );

        _defaultTokenData = new ClientTokenData(
            ScopeCollection.Parse("read write"),
            [],
            TimeSpan.FromMinutes(10));
    }

    public class GenerateToken : JwtTokenGeneratorTests
    {
        [Fact]
        public void ReturnsTokenWithExpectedClaims()
        {
            // Act
            var token = _sut.GenerateToken(_defaultContext, _defaultTokenData, _keyData);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            Assert.Equal("test-issuer", jwt.Issuer);
            Assert.Equal(_defaultContext.Subject, jwt.Subject);
            Assert.Contains(_defaultContext.RequestedAudience.Value, jwt.Audiences);

            var clientIdClaim = jwt.Claims.Single(c => c.Type == "client_id");
            Assert.Equal(_defaultContext.ClientId.ToString(), clientIdClaim.Value);

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
            var now = _time.GetUtcNow().UtcDateTime;
            var lifetime = TimeSpan.FromHours(2);
            var tokenData = _defaultTokenData with { TokenLifetime = TimeSpan.FromHours(2) };

            // Act
            var token = _sut.GenerateToken(_defaultContext, tokenData, _keyData);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            var expectedExpiration = now.Add(lifetime);
            Assert.Equal(expectedExpiration, jwt.ValidTo, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void SetsIssuedAtAndNotBeforeToCurrentTime()
        {
            // Arrange
            var now = _time.GetUtcNow().UtcDateTime;

            // Act
            var token = _sut.GenerateToken(_defaultContext, _defaultTokenData, _keyData);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            Assert.Equal(now, jwt.IssuedAt, TimeSpan.FromSeconds(1));
            Assert.Equal(now, jwt.ValidFrom, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void IncludesJtiClaim()
        {
            // Act
            var token = _sut.GenerateToken(_defaultContext, _defaultTokenData, _keyData);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var jtiClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

            Assert.NotNull(jtiClaim);
            Assert.True(Guid.TryParse(jtiClaim.Value, out _));
        }

        [Fact]
        public void WithEmptyScopes_ReturnsTokenWithNoScopeClaims()
        {
            // Arrange
            var context = _defaultContext with { RequestedScopes = ScopeCollection.Parse("") };

            // Act
            var token = _sut.GenerateToken(context, _defaultTokenData, _keyData);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            Assert.DoesNotContain(jwt.Claims, c => c.Type == "scope");
        }

        [Fact]
        public void WithMultipleScopes_IncludesAllInToken()
        {
            // Arrange
            var scopes = ScopeCollection.Parse("read write delete");
            var context = _defaultContext with { RequestedScopes = scopes };

            // Act
            var token = _sut.GenerateToken(context, _defaultTokenData, _keyData);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var actualScopes = jwt.Claims
                .Where(c => c.Type == "scope")
                .Select(c => c.Value)
                .OrderBy(s => s)
                .ToArray();

            // Assert
            Assert.Equal(["delete", "read", "write"], actualScopes);
        }

        [Fact]
        public void CallsSigningCredentialsFactoryWithProvidedKeyData()
        {
            // Act
            _sut.GenerateToken(_defaultContext, _defaultTokenData, _keyData);

            // Assert
            _credentialsFactory.Received(1).Create(_keyData);
        }

        [Fact]
        public void IncludesKidInHeader()
        {
            // Act
            var token = _sut.GenerateToken(_defaultContext, _defaultTokenData, _keyData);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
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