using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Keys;

namespace OpenAuth.Test.Unit.Security.Keys;

public class HmacSigningKeyStrategyTests
{
    [Fact]
    public void Ctor_Throws_WhenSizeTooSmall()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HmacSigningKeyStrategy(8));
    }

    [Fact]
    public void Ctor_Throws_WhenSizeTooLarge()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HmacSigningKeyStrategy(1024));
    }

    [Fact]
    public void Create_SetsExpectedProperties()
    {
        var strategy = new HmacSigningKeyStrategy(32);
        var expires = DateTime.UtcNow.AddHours(1);

        var key = strategy.Create(expires);

        Assert.Equal(SigningAlgorithm.Hmac, key.Algorithm);
        Assert.NotNull(key.PrivateKey);
        Assert.NotEmpty(key.PrivateKey);
        Assert.Null(key.PublicKey); // HMAC shouldn't have a public key
        Assert.Equal(expires, key.ExpiresAt);
        Assert.NotEqual(default, key.KeyId);

        // Check Base64 validity + length
        var decoded = Convert.FromBase64String(key.PrivateKey);
        Assert.Equal(32, decoded.Length);
    }

    [Fact]
    public void Create_ProducesUniqueKeys()
    {
        var strategy = new HmacSigningKeyStrategy(32);

        var k1 = strategy.Create();
        var k2 = strategy.Create();

        Assert.NotEqual(k1.PrivateKey, k2.PrivateKey);
        Assert.NotEqual(k1.KeyId, k2.KeyId);
    }
}