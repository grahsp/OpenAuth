using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Dtos;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Tokens.SigningCredentials;

namespace OpenAuth.Test.Unit.Security.Signing;

public class SigningCredentialsFactoryTests
{
    private readonly ISigningCredentialsStrategy _rsaStrategy = new RsaSigningCredentialsStrategy();
    private readonly ISigningCredentialsStrategy _hmacStrategy = new HmacSigningCredentialsStrategy();

    public class Constructor : SigningCredentialsFactoryTests
    {
        [Fact]
        public void Throws_WhenNoStrategiesRegistered()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new SigningCredentialsFactory([]));
        }

        [Fact]
        public void Throws_WhenDuplicateStrategyRegistered()
        {
            Assert.Throws<ArgumentException>(() =>
                new SigningCredentialsFactory([_rsaStrategy, _rsaStrategy]));
        }
    }

    public class Create : SigningCredentialsFactoryTests
    {
        [Fact]
        public void Throws_WhenKeyDataIsNull()
        {
            var factory = new SigningCredentialsFactory([_rsaStrategy]);

            Assert.Throws<ArgumentNullException>(() =>
                factory.Create(null!));
        }

        [Fact]
        public void Throws_WhenKeyTypeNotSupported()
        {
            var factory = new SigningCredentialsFactory([_rsaStrategy]);
            
            var keyData = new SigningKeyData(
                SigningKeyId.New(), 
                KeyType.HMAC,  // Not supported - only RSA registered
                SigningAlgorithm.HS256,
                new Key("test-private-key")
            );
            
            Assert.Throws<InvalidOperationException>(() =>
                factory.Create(keyData));
        }
        
        [Fact]
        public void UsesRsaStrategy_WhenKeyTypeIsRsa()
        {
            var factory = new SigningCredentialsFactory([_rsaStrategy, _hmacStrategy]);
            
            var keyData = new SigningKeyData(
                SigningKeyId.New(),
                KeyType.RSA,
                SigningAlgorithm.RS256,
                GenerateRsaPrivateKey()
            );
            
            var credentials = factory.Create(keyData);

            Assert.Equal("RS256", credentials.Algorithm);
            Assert.IsType<RsaSecurityKey>(credentials.Key);
        }

        [Fact]
        public void UsesHmacStrategy_WhenKeyTypeIsHmac()
        {
            var factory = new SigningCredentialsFactory([_rsaStrategy, _hmacStrategy]);
            
            var keyData = new SigningKeyData(
                SigningKeyId.New(), 
                KeyType.HMAC,
                SigningAlgorithm.HS256,
                new Key("my-secret-key-with-256-bits-minimum")
            );
            
            var credentials = factory.Create(keyData);

            Assert.Equal("HS256", credentials.Algorithm);
            Assert.IsType<SymmetricSecurityKey>(credentials.Key);
        }

        private static Key GenerateRsaPrivateKey()
        {
            using var rsa = RSA.Create(2048);
            var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
            return new Key(privateKeyPem);
        }
    }
}