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
}