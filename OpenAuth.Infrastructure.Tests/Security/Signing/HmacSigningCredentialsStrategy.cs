using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Signing;

namespace OpenAuth.Infrastructure.Tests.Security.Signing;

public class HmacSigningCredentialsStrategyTests
{
    [Fact]
    public void Create_Throws_WhenAlgorithmMismatch()
    {
        var key = SigningKey.CreateSymmetric(SigningAlgorithm.Rsa, "secret"); // Wrong algorithm
        var strategy = new HmacSigningCredentialsStrategy();

        Assert.Throws<InvalidOperationException>(() => strategy.GetSigningCredentials(key));
    }

    [Fact]
    public void Create_ReturnsSigningCredentials_WithExpectedProperties()
    {
        var key = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret");
        var strategy = new HmacSigningCredentialsStrategy();

        var signingCredentials = strategy.GetSigningCredentials(key);

        Assert.Equal(SigningAlgorithm.Hmac, key.Algorithm);
        Assert.Equal(key.KeyId.ToString(), signingCredentials.Key.KeyId);
    }
}