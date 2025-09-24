using System.Security.Cryptography;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Security.Signing;

namespace OpenAuth.Test.Unit.Security.Signing;

public class RsaSigningCredentialsStrategyTests
{
    private static SigningKey GenerateRsaKey()
    {
        using var rsa = RSA.Create(2048);
        var pem = PemEncoding.Write("PRIVATE KEY", rsa.ExportPkcs8PrivateKey());
        var key = new Key(new string(pem));

        return new SigningKey(SigningAlgorithm.Rsa, key, DateTime.MinValue, DateTime.MaxValue);
    }

    [Fact]
    public void Create_Throws_WhenAlgorithmMismatch()
    {
        var key = new SigningKey(SigningAlgorithm.Hmac, new Key("secret"), DateTime.MinValue, DateTime.MaxValue);
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
        Assert.Equal(key.Id.ToString(), signingCredentials.Key.KeyId);
    }
}