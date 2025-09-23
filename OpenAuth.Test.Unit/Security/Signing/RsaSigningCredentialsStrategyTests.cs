using System.Security.Cryptography;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Signing;

namespace OpenAuth.Test.Unit.Security.Signing;

public class RsaSigningCredentialsStrategyTests
{
    private static SigningKey GenerateRsaKey()
    {
        using var rsa = RSA.Create(2048);
        var privatePem = PemEncoding.Write("PRIVATE KEY", rsa.ExportPkcs8PrivateKey());
        var publicPem  = PemEncoding.Write("PUBLIC KEY", rsa.ExportSubjectPublicKeyInfo());

        return new SigningKey(SigningAlgorithm.Rsa, new string(privatePem));
    }

    [Fact]
    public void Create_Throws_WhenAlgorithmMismatch()
    {
        var key = new SigningKey(SigningAlgorithm.Hmac, "secret");
        var strategy = new RsaSigningCredentialsStrategy();

        Assert.Throws<InvalidOperationException>(() => strategy.GetSigningCredentials(key));
    }

    [Fact]
    public void Create_ReturnsSigningCredentials_WithExpectedProperties()
    {
        var key = GenerateRsaKey();
        var strategy = new RsaSigningCredentialsStrategy();

        var signingCredentials = strategy.GetSigningCredentials(key);

        Assert.Equal(SigningAlgorithm.Rsa, key.Algorithm);
        Assert.Equal(key.KeyId.ToString(), signingCredentials.Key.KeyId);
    }
}