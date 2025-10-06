using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Dtos;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Tokens.SigningCredentials;

namespace OpenAuth.Test.Unit.Security.Signing;

public class HmacSigningCredentialsStrategyTests
{
    private readonly HmacSigningCredentialsStrategy _strategy = new();

    public class GetSigningCredentials : HmacSigningCredentialsStrategyTests
    {
        [Fact]
        public void Throws_WhenKeyDataIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _strategy.GetSigningCredentials(null!));
        }

        [Fact]
        public void Throws_WhenKeyTypeMismatch()
        {
            var keyData = new SigningKeyData(
                SigningKeyId.New(),
                KeyType.RSA,  // Wrong type for HMAC strategy
                SigningAlgorithm.RS256,
                new Key("some-key")
            );
            
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _strategy.GetSigningCredentials(keyData));
            
            Assert.Contains("RSA", exception.Message);
            Assert.Contains("HMAC", exception.Message);
        }

        [Fact]
        public void ReturnsCredentials_WithCorrectAlgorithm()
        {
            var keyId = SigningKeyId.New();
            var keyData = new SigningKeyData(
                keyId,
                KeyType.HMAC,
                SigningAlgorithm.HS256,
                new Key("my-secret-key-with-at-least-256-bits")
            );

            var credentials = _strategy.GetSigningCredentials(keyData);

            Assert.NotNull(credentials);
            Assert.Equal("HS256", credentials.Algorithm);
            Assert.Equal(keyId.Value.ToString(), credentials.Key.KeyId);
        }

        [Fact]
        public void ReturnsCredentials_WithSymmetricSecurityKey()
        {
            var keyData = new SigningKeyData(
                SigningKeyId.New(),
                KeyType.HMAC,
                SigningAlgorithm.HS256,
                new Key("my-secret-key-with-at-least-256-bits")
            );

            var credentials = _strategy.GetSigningCredentials(keyData);

            Assert.IsType<SymmetricSecurityKey>(credentials.Key);
        }
    }
}