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
        Assert.NotNull(key.Key);
        Assert.Contains("BEGIN PRIVATE KEY", key.Key.Value);
        Assert.Equal(expires, key.ExpiresAt);
        Assert.NotEqual(default, key.Id);
    }

    [Fact]
    public void Create_ProducesUniqueKeys()
    {
        var strategy = new RsaSigningKeyStrategy(2048);

        var k1 = strategy.Create();
        var k2 = strategy.Create();

        Assert.NotEqual(k1.Key, k2.Key);
        Assert.NotEqual(k1.Id, k2.Id);
    }
}