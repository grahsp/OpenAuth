using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Jwks.Dtos;
using OpenAuth.Infrastructure.SigningKeys.Handlers;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.SigningKeys.Infrastructure;

public class RsaSigningKeyHandlerTests
{
    private static readonly RsaSigningKeyHandler Handler = new();
    
    public class CreateJwk
    {
        [Fact]
        public void CreateJwk_WhenInputIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Handler.CreateJwk(null!));
        }
        
        [Fact]
        public void CreateJwk_WhenKeyTypeIsNotRsa_ThrowsInvalidOperationException()
        {
            var signingKey = TestData.CreateValidHmacSigningKey();

            var ex = Assert.Throws<InvalidOperationException>(() => Handler.CreateJwk(signingKey));

            Assert.Contains("HMAC", ex.Message);
            Assert.Contains("RSA", ex.Message);
        }
        
        [Fact]
        public void CreateJwk_FromPrivateKey_ReturnsValidRsaPublicKeyInfo()
        {
            var signingKey = TestData.CreateValidRsaSigningKey();

            var result = Handler.CreateJwk(signingKey);

            var info = Assert.IsType<RsaPublicKeyInfo>(result);
            Assert.NotEmpty(info.N);
            Assert.NotEmpty(info.E);
        }

        [Fact]
        public void CreateJwk_FromPublicKey_ReturnsSameInfoAsPrivateKey()
        {
            var privateKey = TestData.CreateValidRsaSigningKey();
            var fromPrivate = (RsaPublicKeyInfo)Handler.CreateJwk(privateKey);

            var publicPem = CryptoTestUtils.RsaPublicKeyInfoToPem(fromPrivate);
            var publicKey = new SigningKeyBuilder()
                .WithKey(publicPem.Value)
                .AsRsa()
                .Build();

            var fromPublic = (RsaPublicKeyInfo)Handler.CreateJwk(publicKey);

            Assert.Equal(fromPrivate.N, fromPublic.N);
            Assert.Equal(fromPrivate.E, fromPublic.E);
        }
        
        [Fact]
        public void CreateJwk_SameKey_ReturnsSameResults()
        {
            var signingKey = TestData.CreateValidRsaSigningKey();

            var jwk1 = (RsaPublicKeyInfo)Handler.CreateJwk(signingKey);
            var jwk2 = (RsaPublicKeyInfo)Handler.CreateJwk(signingKey);

            Assert.Equal(jwk1.N, jwk2.N);
            Assert.Equal(jwk1.E, jwk2.E);
        }

        [Fact]
        public void CreateJwk_WhenInvalidKeyFormat_ThrowsArgumentException()
        {
            var signingKey = new SigningKeyBuilder()
                .WithKey("invalid-key")
                .Build();

            Assert.Throws<ArgumentException>(() => Handler.CreateJwk(signingKey));
        }
    }

    public class CreateSigningCredentials
    {
        [Fact]
        public void CreateSigningCredentials_WhenInputIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Handler.CreateSigningCredentials(null!));
        }
        
        [Fact]
        public void CreateSigningCredentials_UsesSigningKeyId()
        {
            var signingKey = TestData.CreateValidRsaSigningKey();

            var credentials = Handler.CreateSigningCredentials(signingKey);

            Assert.Equal(signingKey.Id.ToString(), credentials.Key.KeyId);
        }
        
        [Fact]
        public void CreateSigningCredentials_WhenKeyTypeMismatch_ThrowsInvalidOperationException()
        {
            var signingKey = TestData.CreateValidHmacSigningKey();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                Handler.CreateSigningCredentials(signingKey));

            Assert.Contains("HMAC", ex.Message);
            Assert.Contains("RSA", ex.Message);
        }

        [Fact]
        public void CreateSigningCredentials_WhenInvalidKeyFormat_ThrowsArgumentException()
        {
            var signingKey = new SigningKeyBuilder()
                .AsRsa()
                .WithKey("invalid-pem")
                .Build();
            
            Assert.Throws<ArgumentException>(() =>
                Handler.CreateSigningCredentials(signingKey));
        }

        [Fact]
        public void CreateSigningCredentials_AcceptsValidSigningKey()
        {
            var signingKey = TestData.CreateValidRsaSigningKey();

            var credentials = Handler.CreateSigningCredentials(signingKey);

            Assert.NotNull(credentials);
            Assert.Equal("RS256", credentials.Algorithm);
            Assert.Equal(signingKey.Id.ToString(), credentials.Key.KeyId);
        }
        
        [Fact]
        public void CreateSigningCredentials_ReturnsKeyContainingPrivateParameters()
        {
            var signingKey = TestData.CreateValidRsaSigningKey();

            var credentials = Handler.CreateSigningCredentials(signingKey);

            var rsaKey = Assert.IsType<RsaSecurityKey>(credentials.Key);

            Assert.NotNull(rsaKey.Rsa.ExportParameters(true).D); // private exponent
        }

        [Fact]
        public void CreateSigningCredentials_WhenSuccessful_ReturnsCorrectType()
        {
            var signingKey = TestData.CreateValidRsaSigningKey();

            var credentials = Handler.CreateSigningCredentials(signingKey);

            Assert.IsType<RsaSecurityKey>(credentials.Key);
        }
    }

    public class CreateValidationKey
    {
        [Fact]
        public void CreateValidationKey_WhenInputIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Handler.CreateValidationKey(null!));
        }
        
        [Fact]
        public void CreateValidationKey_WhenKeyTypeMismatch_ThrowsInvalidOperationException()
        {
            var signingKey = new SigningKeyBuilder()
                .AsHmac()
                .Build();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                Handler.CreateValidationKey(signingKey));

            Assert.Contains("HMAC", ex.Message);
            Assert.Contains("RSA", ex.Message);
        }

        
        [Fact]
        public void CreateValidationKey_WithValidRsaKey_ReturnsRsaSecurityKey()
        {
            var signingKey = TestData.CreateValidRsaSigningKey();

            var key = Handler.CreateValidationKey(signingKey);

            Assert.IsType<RsaSecurityKey>(key);
        }

        [Fact]
        public void CreateValidationKey_SetsCorrectKeyId()
        {
            var signingKey = TestData.CreateValidRsaSigningKey();

            var key = Handler.CreateValidationKey(signingKey);

            Assert.Equal(signingKey.Id.ToString(), key.KeyId);
        }

        [Fact]
        public void CreateValidationKey_DoesNotContainPrivateParameters()
        {
            var signingKey = TestData.CreateValidRsaSigningKey();
            var rsaKey = (RsaSecurityKey)Handler.CreateValidationKey(signingKey);

            Assert.NotNull(rsaKey.Parameters.Modulus);
            Assert.NotNull(rsaKey.Parameters.Exponent);

            Assert.Null(rsaKey.Parameters.D); // Private exponent must be missing
        }

        [Fact]
        public void CreateValidationKey_UsesCorrectModulusAndExponent()
        {
            var signingKey = TestData.CreateValidRsaSigningKey();
            var (n, e) = RsaSigningKeyHandler.ExtractPublicParameters(signingKey);
        
            var rsaKey = (RsaSecurityKey)Handler.CreateValidationKey(signingKey);
        
            Assert.Equal(rsaKey.Parameters.Modulus, Base64UrlEncoder.DecodeBytes(n));
            Assert.Equal(rsaKey.Parameters.Exponent, Base64UrlEncoder.DecodeBytes(e));
        }
        
        [Fact]
        public void CreateValidationKey_WhenPublicParametersMissing_ThrowsArgumentException()
        {
            var signingKey = new SigningKeyBuilder()
                .AsRsa()
                .WithKey("-----BEGIN RSA PRIVATE KEY----- malformed -----END RSA PRIVATE KEY-----")
                .Build();
        
            Assert.Throws<ArgumentException>(() =>
                Handler.CreateValidationKey(signingKey));
        }
        
        [Fact]
        public void CreateValidationKey_FromPublicKeyPem_ReturnsValidRsaKey()
        {
            var privateKey = TestData.CreateValidRsaSigningKey();
            var publicPem = CryptoTestUtils.RsaPublicKeyInfoToPem(
                (RsaPublicKeyInfo)Handler.CreateJwk(privateKey));
        
            var signingKey = new SigningKeyBuilder()
                .AsRsa()
                .WithKey(publicPem.Value)
                .Build();
        
            var rsaKey = (RsaSecurityKey)Handler.CreateValidationKey(signingKey);
        
            Assert.NotNull(rsaKey.Parameters.Modulus);
            Assert.NotNull(rsaKey.Parameters.Exponent);
        }
    }
}