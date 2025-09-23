using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Signing;

namespace OpenAuth.Test.Unit.Security.Signing;

public class SigningCredentialsFactoryTests
{
    [Fact]
    public void Create_UsesCorrectStrategy_ForHmac()
    {
        var hmacStrategy = new HmacSigningCredentialsStrategy();
        var rsaStrategy = new RsaSigningCredentialsStrategy();
        var factory = new SigningCredentialsFactory([hmacStrategy, rsaStrategy]);

        var key = new SigningKey(SigningAlgorithm.Hmac, "secret");
        var signingCredentials = factory.Create(key);

        Assert.Equal(SecurityAlgorithms.HmacSha256, signingCredentials.Algorithm);
    }

    [Fact]
    public void Create_UsesCorrectStrategy_ForRsa()
    {
        var hmacStrategy = new HmacSigningCredentialsStrategy();
        var rsaStrategy = new RsaSigningCredentialsStrategy();
        var factory = new SigningCredentialsFactory([hmacStrategy, rsaStrategy]);

        using var rsa = RSA.Create(2048);
        var privatePem = PemEncoding.Write("PRIVATE KEY", rsa.ExportPkcs8PrivateKey());
        var publicPem = PemEncoding.Write("PUBLIC KEY", rsa.ExportSubjectPublicKeyInfo());

        var key = new SigningKey(SigningAlgorithm.Rsa, new string(privatePem));
        var signingCredentials = factory.Create(key);

        Assert.Equal(SecurityAlgorithms.RsaSha256, signingCredentials.Algorithm);
    }

    [Fact]
    public void Create_Throws_WhenStrategyNotRegistered()
    {
        var factory = new SigningCredentialsFactory([]);
        var key = new SigningKey(SigningAlgorithm.Hmac, "secret");

        Assert.Throws<InvalidOperationException>(() => factory.Create(key));
    }
}