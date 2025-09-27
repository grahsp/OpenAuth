using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Keys;

namespace OpenAuth.Test.Unit.Security.Keys;

public class RsaKeyMaterialGeneratorTests
{
    [Fact]
    public void Ctor_Throws_WhenSizeTooSmall()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RsaKeyMaterialGenerator(1024));
    }
    
    [Fact]
    public void Ctor_Throws_WhenSizeTooLarge()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RsaKeyMaterialGenerator(10_000));
    }

    [Fact]
    public void Create_SetsExpectedProperties()
    {
        var strategy = new RsaKeyMaterialGenerator(2048);
        var expires = DateTime.UtcNow.AddDays(1);

        var keyMaterial = strategy.Create(SigningAlgorithm.RS256);

        Assert.NotNull(keyMaterial.Key);
        Assert.Contains("BEGIN PRIVATE KEY", keyMaterial.Key.Value);
        Assert.Equal(SigningAlgorithm.RS256, keyMaterial.Alg);
        Assert.Equal(KeyType.RSA, keyMaterial.Kty);
    }

    [Fact]
    public void Create_ProducesUniqueKeys()
    {
        var strategy = new RsaKeyMaterialGenerator(2048);

        var k1 = strategy.Create(SigningAlgorithm.RS256);
        var k2 = strategy.Create(SigningAlgorithm.RS256);

        Assert.NotEqual(k1.Key, k2.Key);
    }
}