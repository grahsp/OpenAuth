using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Keys;

namespace OpenAuth.Test.Unit.Security.Keys;

public class HmacKeyMaterialGeneratorTests
{
    [Fact]
    public void Ctor_Throws_WhenSizeOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HmacKeyMaterialGenerator(HmacKeyMaterialGenerator.MinSize - 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HmacKeyMaterialGenerator(HmacKeyMaterialGenerator.MaxSize + 1));
    }
    
    [Fact]
    public void Ctor_AllowsMinAndMaxSizes()
    {
        _ = new HmacKeyMaterialGenerator(HmacKeyMaterialGenerator.MinSize);
        _ = new HmacKeyMaterialGenerator(HmacKeyMaterialGenerator.MaxSize);
    }

    [Fact]
    public void Create_SetsExpectedProperties()
    {
        var strategy = new HmacKeyMaterialGenerator(HmacKeyMaterialGenerator.MinSize);
        
        var alg = SigningAlgorithm.HS256;
        var keyMaterial = strategy.Create(alg);

        Assert.NotNull(keyMaterial.Key);
        Assert.NotEmpty(keyMaterial.Key.Value);
        Assert.Equal(alg, keyMaterial.Alg);
        Assert.Equal(KeyType.HMAC, keyMaterial.Kty);

        // Check Base64 validity + length
        var decoded = Convert.FromBase64String(keyMaterial.Key.Value);
        Assert.Equal(32, decoded.Length);
    }

    [Fact]
    public void Create_SetsExpectedSize()
    {
        const int expectedBytes = HmacKeyMaterialGenerator.MinSize;
        var strategy = new HmacKeyMaterialGenerator(expectedBytes);
        var keyMaterial = strategy.Create(SigningAlgorithm.HS256);

        var rawBytes = Convert.FromBase64String(keyMaterial.Key.Value);
        Assert.Equal(expectedBytes, rawBytes.Length);
    }
    
    [Fact]
    public void Create_ReturnsValidBase64()
    {
        var strategy = new HmacKeyMaterialGenerator(HmacKeyMaterialGenerator.MinSize);
        var keyMaterial = strategy.Create(SigningAlgorithm.HS256);

        var ex =Record.Exception(() => Convert.FromBase64String(keyMaterial.Key.Value));
        
        Assert.Null(ex);
    }

    [Fact]
    public void Create_ProducesUniqueKeys()
    {
        var strategy = new HmacKeyMaterialGenerator(HmacKeyMaterialGenerator.MinSize);

        var k1 = strategy.Create(SigningAlgorithm.HS256);
        var k2 = strategy.Create(SigningAlgorithm.HS256);

        Assert.NotEqual(k1.Key, k2.Key);
    }
}