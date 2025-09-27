using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Keys;

namespace OpenAuth.Test.Unit.Security.Keys;

public class HmacKeyMaterialGeneratorTests
{
    [Fact]
    public void Ctor_Throws_WhenSizeTooSmall()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HmacKeyMaterialGenerator(8));
    }

    [Fact]
    public void Ctor_Throws_WhenSizeTooLarge()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HmacKeyMaterialGenerator(1024));
    }

    [Fact]
    public void Create_SetsExpectedProperties()
    {
        var strategy = new HmacKeyMaterialGenerator(32);
        var expires = DateTime.UtcNow.AddHours(1);

        var keyMaterial = strategy.Create(SigningAlgorithm.HM256);

        Assert.NotNull(keyMaterial.Key);
        Assert.NotEmpty(keyMaterial.Key.Value);
        Assert.Equal(SigningAlgorithm.HM256, keyMaterial.Alg);
        Assert.Equal(KeyType.HMAC, keyMaterial.Kty);

        // Check Base64 validity + length
        var decoded = Convert.FromBase64String(keyMaterial.Key.Value);
        Assert.Equal(32, decoded.Length);
    }

    [Fact]
    public void Create_ProducesUniqueKeys()
    {
        var strategy = new HmacKeyMaterialGenerator(32);

        var k1 = strategy.Create(SigningAlgorithm.HM256);
        var k2 = strategy.Create(SigningAlgorithm.HM256);

        Assert.NotEqual(k1.Key, k2.Key);
    }
}