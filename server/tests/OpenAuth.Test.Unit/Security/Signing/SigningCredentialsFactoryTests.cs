using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Infrastructure.SigningKeys.Handlers;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.Security.Signing;

public class SigningCredentialsFactoryTests
{
    private readonly ISigningKeyHandler _rsaHandler = new RsaSigningKeyHandler();
    private readonly ISigningKeyHandler _hmacHandler = new HmacSigningKeyHandler();

    [Fact]
    public void Throws_WhenNoStrategiesRegistered()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new SigningCredentialsFactory([]));
    }

    [Fact]
    public void Throws_WhenDuplicateStrategyRegistered()
    {
        Assert.Throws<ArgumentException>(() =>
            new SigningCredentialsFactory([_rsaHandler, _rsaHandler]));
    }

    [Fact]
    public void Throws_WhenKeyDataIsNull()
    {
        var factory = new SigningCredentialsFactory([_rsaHandler]);

        Assert.Throws<ArgumentNullException>(() =>
            factory.Create(null!));
    }

    [Fact]
    public void Throws_WhenKeyTypeNotSupported()
    {
        var factory = new SigningCredentialsFactory([_rsaHandler]);
        var signingKey = TestData.CreateValidHmacSigningKey(); // Only RSA handler registered.
            
        Assert.Throws<InvalidOperationException>(() =>
            factory.Create(signingKey));
    }
        
    [Fact]
    public void UsesRsaStrategy_WhenKeyTypeIsRsa()
    {
        var factory = new SigningCredentialsFactory([_rsaHandler, _hmacHandler]);
        var signingKey = TestData.CreateValidRsaSigningKey();
            
        var credentials = factory.Create(signingKey);

        Assert.Equal("RS256", credentials.Algorithm);
        Assert.IsType<RsaSecurityKey>(credentials.Key);
    }

    [Fact]
    public void UsesHmacStrategy_WhenKeyTypeIsHmac()
    {
        var factory = new SigningCredentialsFactory([_rsaHandler, _hmacHandler]);
        var signingKey = TestData.CreateValidHmacSigningKey();
            
        var credentials = factory.Create(signingKey);

        Assert.Equal("HS256", credentials.Algorithm);
        Assert.IsType<SymmetricSecurityKey>(credentials.Key);
    }

    private static Key GenerateRsaPrivateKey()
    {
        using var rsa = RSA.Create(2048);
        var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
        return new Key(privateKeyPem);
    }
}