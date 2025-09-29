using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Keys;

namespace OpenAuth.Test.Unit.Security.Keys;

public class SigningKeyFactoryTests
{
    private readonly IKeyMaterialGenerator _rsaGenerator =
        new RsaKeyMaterialGenerator(RsaKeyMaterialGenerator.MinSize);
    private readonly IKeyMaterialGenerator _hmacGenerator =
        new HmacKeyMaterialGenerator(HmacKeyMaterialGenerator.MinSize);
    
    private readonly TimeProvider _time = new FakeTimeProvider();

    [Fact]
    public void Ctor_Throws_WhenNoGeneratorsRegistered()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new SigningKeyFactory([]));
    }

    [Fact]
    public void Ctor_Throws_WhenDuplicateGeneratorRegistered()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new SigningKeyFactory([_rsaGenerator, _rsaGenerator]));
    }

    [Fact]
    public void Create_Throws_WhenCreatedAtIsDefault()
    {
        var factory = new SigningKeyFactory([_rsaGenerator]);
        
        Assert.Throws<ArgumentException>(() =>
            factory.Create(SigningAlgorithm.RS256, default));
    }
    
    [Fact]
    public void Create_Throws_WhenLifetimeIsNonPositive()
    {
        var factory = new SigningKeyFactory([_rsaGenerator]);
        
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            factory.Create(SigningAlgorithm.RS256, _time.GetUtcNow().UtcDateTime, TimeSpan.Zero));
        
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            factory.Create(SigningAlgorithm.RS256, _time.GetUtcNow().UtcDateTime, TimeSpan.FromSeconds(-1)));
    }
    
    [Fact]
    public void Create_Throws_WhenAlgorithmNotSupported()
    {
        var factory = new SigningKeyFactory([_rsaGenerator]);
        
        Assert.Throws<InvalidOperationException>(() =>
            factory.Create(SigningAlgorithm.HS256, _time.GetUtcNow().UtcDateTime));
    }

    [Fact]
    public void Create_SetsExpectedProperties_WithDefaultLifetime()
    {
        var factory = new SigningKeyFactory([_rsaGenerator]);
        
        var now = _time.GetUtcNow().UtcDateTime;
        const SigningAlgorithm alg = SigningAlgorithm.RS256;
        
        var signingKey = factory.Create(alg, now);

        Assert.NotNull(signingKey.KeyMaterial);
        Assert.Equal(alg, signingKey.KeyMaterial.Alg);
        Assert.Equal(KeyType.RSA, signingKey.KeyMaterial.Kty);
        Assert.Equal(now, signingKey.CreatedAt);
        Assert.Equal(now.AddDays(SigningKeyFactory.DefaultLifetimeInDays), signingKey.ExpiresAt);
    }
    
    [Fact]
    public void Create_SetsExpectedProperties_WithCustomLifetime()
    {
        var factory = new SigningKeyFactory([_rsaGenerator]);
        
        var now = _time.GetUtcNow().UtcDateTime;
        var lifetime = TimeSpan.FromDays(1);
        const SigningAlgorithm alg = SigningAlgorithm.RS256;
        
        var signingKey = factory.Create(alg, now, lifetime);

        Assert.NotNull(signingKey.KeyMaterial);
        Assert.Equal(alg, signingKey.KeyMaterial.Alg);
        Assert.Equal(KeyType.RSA, signingKey.KeyMaterial.Kty);
        Assert.Equal(now, signingKey.CreatedAt);
        Assert.Equal(now.Add(lifetime), signingKey.ExpiresAt);
    }

    [Fact]
    public void Create_UsesCorrectGeneratorForAlgorithm()
    {
        var factory = new SigningKeyFactory([_rsaGenerator, _hmacGenerator]);
        
        var now = _time.GetUtcNow().UtcDateTime;
        var rsaKey = factory.Create(SigningAlgorithm.RS256, now);
        var hmacKey = factory.Create(SigningAlgorithm.HS256, now);
        
        Assert.Equal(KeyType.RSA, rsaKey.KeyMaterial.Kty);
        Assert.Equal(KeyType.HMAC, hmacKey.KeyMaterial.Kty);
    }
}