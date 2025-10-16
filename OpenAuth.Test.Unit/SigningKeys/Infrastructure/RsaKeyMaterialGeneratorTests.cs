using System.Security.Cryptography;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Infrastructure.SigningKeys.KeyMaterials;

namespace OpenAuth.Test.Unit.SigningKeys.Infrastructure;

public class RsaKeyMaterialGeneratorTests
{
    [Fact]
    public void Ctor_Throws_WhenSizeNotMultipleOfEight()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RsaKeyMaterialGenerator(RsaKeyMaterialGenerator.MinSize + 1));
    }
    
    [Fact]
    public void Ctor_Throws_WhenSizeOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RsaKeyMaterialGenerator(RsaKeyMaterialGenerator.MinSize - 8));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RsaKeyMaterialGenerator(RsaKeyMaterialGenerator.MaxSize + 8));
    }

    [Fact]
    public void Ctor_AllowsMinAndMaxSizes()
    {
        _ = new RsaKeyMaterialGenerator(RsaKeyMaterialGenerator.MinSize);
        _ = new RsaKeyMaterialGenerator(RsaKeyMaterialGenerator.MaxSize);
    }

    [Fact]
    public void Create_SetsExpectedProperties()
    {
        var strategy = new RsaKeyMaterialGenerator(RsaKeyMaterialGenerator.MinSize);
        
        const SigningAlgorithm alg = SigningAlgorithm.RS256;
        var keyMaterial = strategy.Create(alg);

        Assert.NotNull(keyMaterial.Key);
        Assert.Contains("BEGIN PRIVATE KEY", keyMaterial.Key.Value);
        Assert.Contains("END PRIVATE KEY", keyMaterial.Key.Value);
        Assert.Equal(alg, keyMaterial.Alg);
        Assert.Equal(KeyType.RSA, keyMaterial.Kty);
    }

    [Fact]
    public void Create_SetsExpectedSize()
    {
        const int expected = RsaKeyMaterialGenerator.MinSize;
        var strategy = new RsaKeyMaterialGenerator(expected);
        var keyMaterial = strategy.Create(SigningAlgorithm.RS256);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(keyMaterial.Key.Value);
        var actual = rsa.KeySize;
        
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void Create_ReturnsValidPem()
    {
        var strategy = new RsaKeyMaterialGenerator(RsaKeyMaterialGenerator.MinSize);
        var keyMaterial = strategy.Create(SigningAlgorithm.RS256);

        var ex = Record.Exception(() =>
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(keyMaterial.Key.Value);
        });
        
        Assert.Null(ex);
    }

    [Fact]
    public void Create_ProducesUniqueKeys()
    {
        var strategy = new RsaKeyMaterialGenerator(RsaKeyMaterialGenerator.MinSize);

        var k1 = strategy.Create(SigningAlgorithm.RS256);
        var k2 = strategy.Create(SigningAlgorithm.RS256);

        Assert.NotEqual(k1.Key, k2.Key);
    }
}