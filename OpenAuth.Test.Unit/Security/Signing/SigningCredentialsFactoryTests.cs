using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Security.Signing;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.Security.Signing;

public class SigningCredentialsFactoryTests
{
    [Fact]
    public void Create_UsesCorrectStrategy_ForHmac()
    {
        var hmacStrategy = new HmacSigningCredentialsStrategy();
        var rsaStrategy = new RsaSigningCredentialsStrategy();
        var factory = new SigningCredentialsFactory([hmacStrategy, rsaStrategy]);

        var signingKey = TestSigningKey.CreateHmacSigningKey();
        var signingCredentials = factory.Create(signingKey);

        Assert.Equal(SecurityAlgorithms.HmacSha256, signingCredentials.Algorithm);
    }

    [Fact]
    public void Create_UsesCorrectStrategy_ForRsa()
    {
        var hmacStrategy = new HmacSigningCredentialsStrategy();
        var rsaStrategy = new RsaSigningCredentialsStrategy();
        var factory = new SigningCredentialsFactory([hmacStrategy, rsaStrategy]);

        using var rsa = RSA.Create(2048);
        var pem = PemEncoding.Write("PRIVATE KEY", rsa.ExportPkcs8PrivateKey());
        var key = new Key(new string(pem));

        var signingKey = TestSigningKey.CreateRsaSigningKey(key);
        var signingCredentials = factory.Create(signingKey);

        Assert.Equal(SecurityAlgorithms.RsaSha256, signingCredentials.Algorithm);
    }

    // [Fact]
    // public void Create_Throws_WhenStrategyNotRegistered()
    // {
    //     var factory = new SigningCredentialsFactory([]);
    //     var signingKey = TestSigningKey.CreateHmacSigningKey();
    //
    //     Assert.Throws<InvalidOperationException>(() => factory.Create(signingKey));
    // }
}