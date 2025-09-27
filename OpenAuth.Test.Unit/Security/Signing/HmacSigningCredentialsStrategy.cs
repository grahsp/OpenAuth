using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Signing;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.Security.Signing;

public class HmacSigningCredentialsStrategyTests
{
    [Fact]
    public void Create_Throws_WhenAlgorithmMismatch()
    {
        var signingKey = TestSigningKey.CreateRsaSigningKey(); // Wrong algorithm
        var strategy = new HmacSigningCredentialsStrategy();

        Assert.Throws<InvalidOperationException>(() => strategy.GetSigningCredentials(signingKey.Id.ToString(), signingKey.KeyMaterial));
    }

    [Fact]
    public void Create_ReturnsSigningCredentials_WithExpectedProperties()
    {
        var signingKey = TestSigningKey.CreateHmacSigningKey(algorithm: SigningAlgorithm.HM256); // Wrong algorithm
        var strategy = new HmacSigningCredentialsStrategy();

        var signingCredentials = strategy.GetSigningCredentials(signingKey.Id.ToString(), signingKey.KeyMaterial);

        Assert.Equal(SigningAlgorithm.HM256, signingKey.KeyMaterial.Alg);
        Assert.Equal(signingKey.Id.ToString(), signingCredentials.Key.KeyId);
    }
}