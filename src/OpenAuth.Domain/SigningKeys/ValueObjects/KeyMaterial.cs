using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.Extensions;

namespace OpenAuth.Domain.SigningKeys.ValueObjects;

public sealed record KeyMaterial
{
    public Key Key { get; }
    public SigningAlgorithm Alg { get; }
    public KeyType Kty { get; }
    
    public KeyMaterial(Key key, SigningAlgorithm alg, KeyType kty)
    {
        if (alg.ToKeyType() != kty)
            throw new ArgumentException(
                $"Algorithm {alg} does not match declared key type {kty}.", nameof(alg));
        
        Key = key;
        Alg = alg;
        Kty = kty;
    }
}