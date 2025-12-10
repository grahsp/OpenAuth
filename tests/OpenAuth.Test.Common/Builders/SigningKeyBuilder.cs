using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public class SigningKeyBuilder
{
    private Key _key = new(DefaultValues.PrivatePem);
    private KeyType _kty = KeyType.RSA;
    private SigningAlgorithm _alg = SigningAlgorithm.RS256;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private DateTimeOffset? _expiresAt;
    
    public SigningKeyBuilder WithKey(string key)
    {
        _key = new Key(key);
        return this;
    }

    public SigningKeyBuilder WithKty(KeyType kty)
    {
        _kty = kty;
        return this;
    }
    
    public SigningKeyBuilder WithAlg(SigningAlgorithm alg)
    {
        _alg = alg;
        return this;
    }

    public SigningKeyBuilder CreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public SigningKeyBuilder ExpiresAt(DateTimeOffset expiresAt)
    {
        _expiresAt = expiresAt;
        return this;
    }

    public SigningKeyBuilder AsRsa()
    {
        _kty = KeyType.RSA;
        _alg = SigningAlgorithm.RS256;
        _key = new Key(DefaultValues.PrivatePem);
            
        return this;
    }

    public SigningKeyBuilder AsHmac()
    {
        _kty = KeyType.HMAC;
        _alg = SigningAlgorithm.HS256;
        _key = new Key(DefaultValues.Secret);
        
        return this;
    }

    public SigningKey Build()
    {
        var keyMaterial = new KeyMaterial(_key, _alg, _kty);
        var expiresAt = _expiresAt ?? _createdAt.AddHours(24);
        
        return new SigningKey(keyMaterial, _createdAt, expiresAt);
    }
}