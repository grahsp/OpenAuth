using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Signing;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.Security.Signing;

public class RsaSigningCredentialsStrategyTests
{
    [Fact]
    public void Create_Throws_WhenAlgorithmMismatch()
    {
        var signingKey = TestSigningKey.CreateHmacSigningKey();
        var strategy = new RsaSigningCredentialsStrategy();

        Assert.Throws<InvalidOperationException>(() => strategy.GetSigningCredentials(signingKey.Id.ToString(), signingKey.KeyMaterial));
    }

    [Fact]
    public void Create_ReturnsSigningCredentials_WithExpectedProperties()
    {
        var signingKey = TestSigningKey.CreateRsaSigningKey(algorithm: SigningAlgorithm.RS256);
        var strategy = new RsaSigningCredentialsStrategy();

        var signingCredentials = strategy.GetSigningCredentials(signingKey.Id.ToString(), signingKey.KeyMaterial);

        Assert.Equal(signingKey.Id.ToString(), signingCredentials.Key.KeyId);
        Assert.Equal(SigningAlgorithm.RS256, signingKey.KeyMaterial.Alg);
    }
}