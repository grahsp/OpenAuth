using System.Text;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Infrastructure.SigningKeys.Handlers;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.SigningKeys.Infrastructure;

public class HmacSigningKeyHandlerTests
{
    private static readonly HmacSigningKeyHandler Handler = new();

    public class CreateJwk
    {
        [Fact]
        public void CreateJwk_ThrowsNotSupportedException()
        {
            var signingKey = TestData.CreateValidHmacSigningKey();

            Assert.Throws<NotSupportedException>(() =>
                Handler.CreateJwk(signingKey));
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
        public void CreateSigningCredentials_WhenKeyTypeMismatch_ThrowsInvalidOperationException()
        {
            var signingKey = TestData.CreateValidRsaSigningKey();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                Handler.CreateSigningCredentials(signingKey));

            Assert.Contains("HMAC", ex.Message);
            Assert.Contains("RSA", ex.Message);
        } 
        
        [Fact]
        public void CreateSigningCredentials_WhenKeyIsEmpty_ThrowsArgumentException()
        {
            var signingKey = new SigningKeyBuilder()
                .AsHmac()
                .WithKey("") // invalid HMAC key
                .Build();

            Assert.Throws<ArgumentException>(() =>
                Handler.CreateSigningCredentials(signingKey));
        }

        [Fact]
        public void CreateSigningCredentials_WithValidSigningKey_ReturnsSigningCredentials()
        {
            var signingKey = TestData.CreateValidHmacSigningKey();

            var credentials = Handler.CreateSigningCredentials(signingKey);

            Assert.NotNull(credentials);
            Assert.Equal("HS256", credentials.Algorithm);
            Assert.Equal(signingKey.Id.ToString(), credentials.Key.KeyId);
        }

        [Fact]
        public void CreateSigningCredentials_WhenSuccessful_ReturnsCorrectType()
        {
            var signingKey = TestData.CreateValidHmacSigningKey();

            var credentials = Handler.CreateSigningCredentials(signingKey);

            Assert.IsType<SymmetricSecurityKey>(credentials.Key);
        }
    }

    public class CreateValidationKey
    {
        [Fact]
        public void CreateValidationKey_WhenSigningKeyIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Handler.CreateValidationKey(null!));
        }
        
        [Fact]
        public void CreateValidationKey_WhenKeyTypeMismatch_ThrowsInvalidOperationException()
        {
            var signingKey = new SigningKeyBuilder()
                .AsRsa()
                .Build();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                Handler.CreateValidationKey(signingKey));

            Assert.Contains("Handler", ex.Message);
            Assert.Contains("RSA", ex.Message);
            Assert.Contains("HMAC", ex.Message);
        }
        
        [Fact]
        public void CreateValidationKey_WithValidKey_ReturnsSymmetricSecurityKey()
        {
            var signingKey = TestData.CreateValidHmacSigningKey();

            var result = Handler.CreateValidationKey(signingKey);

            var key = Assert.IsType<SymmetricSecurityKey>(result);
            Assert.Equal(signingKey.Id.ToString(), key.KeyId);
        }
        
        [Fact]
        public void CreateValidationKey_UsesRawKeyBytesFromKeyMaterial()
        {
            const string rawKey = "my-secret-value";
    
            var signingKey = new SigningKeyBuilder()
                .AsHmac()
                .WithKey(rawKey)
                .Build();

            var securityKey = (SymmetricSecurityKey)Handler.CreateValidationKey(signingKey);

            Assert.Equal(Encoding.UTF8.GetBytes(rawKey), securityKey.Key);
        }
        
        [Fact]
        public void CreateValidationKey_WhenKeyIsEmpty_ThrowsArgumentException()
        {
            var signingKey = new SigningKeyBuilder()
                .AsHmac()
                .WithKey("")
                .Build();

            Assert.Throws<ArgumentException>(() =>
                Handler.CreateValidationKey(signingKey));
        }
    }
}