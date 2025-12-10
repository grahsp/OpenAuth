using Microsoft.IdentityModel.Tokens;
using OpenAuth.Infrastructure.SigningKeys.Handlers;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.SigningKeys.Infrastructure;

public class HmacSigningKeyHandlerTests
{
    private static readonly HmacSigningKeyHandler Handler = new();
    
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
}