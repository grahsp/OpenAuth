using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Keys;

namespace OpenAuth.Test.Unit.Security.Keys;

public class SigningKeyFactoryTests
{
    [Fact]
    public void Create_UsesCorrectStrategy_ForHmac()
    {
        var hmac = new HmacSigningKeyStrategy(32);
        var rsa  = new RsaSigningKeyStrategy(2048);

        var factory = new SigningKeyFactory([hmac, rsa]);
        var key = factory.Create(SigningAlgorithm.Hmac);

        Assert.Equal(SigningAlgorithm.Hmac, key.Algorithm);
    }

    [Fact]
    public void Create_UsesCorrectStrategy_ForRsa()
    {
        var hmac = new HmacSigningKeyStrategy(32);
        var rsa  = new RsaSigningKeyStrategy(2048);

        var factory = new SigningKeyFactory([hmac, rsa]);
        var key = factory.Create(SigningAlgorithm.Rsa);

        Assert.Equal(SigningAlgorithm.Rsa, key.Algorithm);
    }

    [Fact]
    public void Create_Throws_WhenStrategyNotRegistered()
    {
        var factory = new SigningKeyFactory([]);

        Assert.Throws<InvalidOperationException>(() => factory.Create(SigningAlgorithm.Hmac));
    }
}