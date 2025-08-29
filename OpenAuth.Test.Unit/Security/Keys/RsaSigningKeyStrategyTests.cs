using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Keys;

namespace OpenAuth.Test.Unit.Security.Keys;

public class RsaSigningKeyStrategyTests
{
    [Fact]
    public void Ctor_Throws_WhenSizeTooSmall()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RsaSigningKeyStrategy(1024));
    }
    
    [Fact]
    public void Ctor_Throws_WhenSizeTooLarge()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RsaSigningKeyStrategy(10_000));
    }

    [Fact]
    public void Create_SetsExpectedProperties()
    {
        var strategy = new RsaSigningKeyStrategy(2048);
        var expires = DateTime.UtcNow.AddDays(1);

        var key = strategy.Create(expires);

        Assert.Equal(SigningAlgorithm.Rsa, key.Algorithm);
        Assert.NotNull(key.PrivateKey);
        Assert.NotNull(key.PublicKey);
        Assert.Contains("BEGIN PRIVATE KEY", key.PrivateKey);
        Assert.Contains("BEGIN PUBLIC KEY", key.PublicKey);
        Assert.Equal(expires, key.ExpiresAt);
        Assert.NotEqual(default, key.KeyId);
    }

    [Fact]
    public void Create_ProducesUniqueKeys()
    {
        var strategy = new RsaSigningKeyStrategy(2048);

        var k1 = strategy.Create();
        var k2 = strategy.Create();

        Assert.NotEqual(k1.PrivateKey, k2.PrivateKey);
        Assert.NotEqual(k1.KeyId, k2.KeyId);
    }
}