using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Infrastructure.Security.Extensions;
using OpenAuth.Infrastructure.Security.Signing;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.Security.Signing;

public class SigningCredentialsFactoryTests
{
    private readonly ISigningCredentialsStrategy _rsaStrategy =
        new RsaSigningCredentialsStrategy();
    private readonly ISigningCredentialsStrategy _hmacStrategy =
        new HmacSigningCredentialsStrategy();
    
    private readonly TimeProvider _time = new FakeTimeProvider();

    [Fact]
    public void Ctor_Throws_WhenNoStrategiesRegistered()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new SigningCredentialsFactory([]));
    }

    [Fact]
    public void Ctor_Throws_WhenDuplicateStrategyRegistered()
    {
        Assert.Throws<ArgumentException>(() =>
            new SigningCredentialsFactory([_rsaStrategy, _rsaStrategy]));
    }
    
    [Fact]
    public void Create_Throws_WhenSigningKeyIsNull()
    {
        var factory = new SigningCredentialsFactory([_rsaStrategy]);

        Assert.Throws<ArgumentNullException>(() =>
            factory.Create(null!));
    }

    [Fact]
    public void Create_Throws_WhenAlgorithmNotSupported()
    {
        var factory = new SigningCredentialsFactory([_rsaStrategy]);
        var signingKey = TestSigningKey.CreateHmacSigningKey(_time);
        
        Assert.Throws<InvalidOperationException>(() =>
            factory.Create(signingKey));
    }
    
    [Fact]
    public void Create_UsesCorrectStrategyForAlgorithm()
    {
        var factory = new SigningCredentialsFactory([_rsaStrategy, _hmacStrategy]);
        var hmacKey = TestSigningKey.CreateHmacSigningKey(_time);
        var rsaKey = TestSigningKey.CreateRsaSigningKey(_time);

        var hmacCredentials = factory.Create(hmacKey);
        var rsaCredentials = factory.Create(rsaKey);

        Assert.Equal(hmacKey.KeyMaterial.Alg.ToSecurityString(), hmacCredentials.Algorithm);
        Assert.Equal(rsaKey.KeyMaterial.Alg.ToSecurityString(), rsaCredentials.Algorithm);
    }
}