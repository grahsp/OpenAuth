using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Dtos;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Tokens.SigningCredentials;

namespace OpenAuth.Test.Unit.Security.Signing;

public class RsaSigningCredentialsStrategyTests
{
    private readonly RsaSigningCredentialsStrategy _strategy = new();

    public class GetSigningCredentials : RsaSigningCredentialsStrategyTests
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
                KeyType.HMAC,  // Wrong type for RSA strategy
                SigningAlgorithm.HS256,
                new Key("some-key")
            );
            
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _strategy.GetSigningCredentials(keyData));
            
            Assert.Contains("HMAC", exception.Message);
            Assert.Contains("RSA", exception.Message);
        }

        [Fact]
        public void Throws_WhenPrivateKeyIsInvalid()
        {
            var keyData = new SigningKeyData(
                SigningKeyId.New(),
                KeyType.RSA,
                SigningAlgorithm.RS256,
                new Key("invalid-base64-key-data!!!")
            );
            
            Assert.Throws<ArgumentException>(() =>
                _strategy.GetSigningCredentials(keyData));
        }

        [Fact]
        public void Throws_WhenPrivateKeyIsNotValidRsa()
        {
            var invalidRsaKey = Convert.ToBase64String(
                "not-rsa-key-data"u8);
            
            var keyData = new SigningKeyData(
                SigningKeyId.New(),
                KeyType.RSA,
                SigningAlgorithm.RS256,
                new Key(invalidRsaKey)
            );
            
            Assert.ThrowsAny<ArgumentException>(() =>
                _strategy.GetSigningCredentials(keyData));
        }

        [Fact]
        public void ReturnsCredentials_WithCorrectAlgorithm()
        {
            var keyId = SigningKeyId.New();
            var keyData = new SigningKeyData(
                keyId,
                KeyType.RSA,
                SigningAlgorithm.RS256,
                GenerateRsaPrivateKey()
            );

            var credentials = _strategy.GetSigningCredentials(keyData);

            Assert.NotNull(credentials);
            Assert.Equal("RS256", credentials.Algorithm);
            Assert.Equal(keyId.Value.ToString(), credentials.Key.KeyId);
        }

        [Fact]
        public void ReturnsCredentials_WithRsaSecurityKey()
        {
            var keyData = new SigningKeyData(
                SigningKeyId.New(),
                KeyType.RSA,
                SigningAlgorithm.RS256,
                GenerateRsaPrivateKey()
            );

            var credentials = _strategy.GetSigningCredentials(keyData);

            Assert.IsType<RsaSecurityKey>(credentials.Key);
        }

        private static Key GenerateRsaPrivateKey()
        {
            using var rsa = RSA.Create(2048);
            var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
            return new Key(privateKeyPem);
        }
    }
}