using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Keys;

namespace OpenAuth.Test.Unit.Security.Keys;

public class SigningKeyFactoryTests
{
    [Fact]
    public void Create_UsesCorrectStrategy_ForHmac()
    {
        var hmac = new HmacKeyMaterialGenerator(32);
        var rsa  = new RsaKeyMaterialGenerator(2048);

        var factory = new SigningKeyFactory([hmac, rsa]);
        var signingKey = factory.Create(SigningAlgorithm.HM256, DateTime.UtcNow);

        Assert.Equal(SigningAlgorithm.HM256, signingKey.KeyMaterial.Alg);
    }

    [Fact]
    public void Create_UsesCorrectStrategy_ForRsa()
    {
        var hmac = new HmacKeyMaterialGenerator(32);
        var rsa  = new RsaKeyMaterialGenerator(2048);

        var factory = new SigningKeyFactory([hmac, rsa]);
        var signingKey = factory.Create(SigningAlgorithm.RS256, DateTime.UtcNow);

        Assert.Equal(SigningAlgorithm.RS256, signingKey.KeyMaterial.Alg);
    }

    // [Fact]
    // public void Create_Throws_WhenStrategyNotRegistered()
    // {
    //     var factory = new SigningKeyFactory([]);
    //     Assert.Throws<InvalidOperationException>(() => factory.Create(SigningAlgorithm.HM256, DateTime.MinValue));
    // }
}