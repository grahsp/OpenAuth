using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.Extensions;

namespace OpenAuth.Application.SigningKeys;

public class SigningKeyFactory : ISigningKeyFactory
{
    private readonly Dictionary<KeyType, IKeyMaterialGenerator> _generators;
    public const int DefaultLifetimeInDays = 30;
    
    public SigningKeyFactory(IEnumerable<IKeyMaterialGenerator> generators)
    {
        try
        {
            _generators = generators
                .ToDictionary(g => g.KeyType);
        }
        catch (ArgumentException e)
        {
            throw new InvalidOperationException("Duplicate algorithm registered in generators", e);
        }
        
        if (_generators.Count == 0)
            throw new InvalidOperationException("No generator registered.");
    }

    public SigningKey Create(SigningAlgorithm algorithm, DateTimeOffset createdAt, TimeSpan? lifetime = null)
    {
        if (createdAt == default)
            throw new ArgumentException("Creation date must be explicitly set.", nameof(createdAt));
        
        if (lifetime.HasValue && lifetime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Lifetime must be positive.");
        
        if (!_generators.TryGetValue(algorithm.ToKeyType(), out var generator))
            throw new InvalidOperationException($"Unsupported algorithm used { algorithm }.");

        var keyMaterial = generator.Create(algorithm);
        var expiresAt = createdAt.Add(lifetime ?? TimeSpan.FromDays(DefaultLifetimeInDays));
        
        return new SigningKey(keyMaterial, createdAt, expiresAt);
    }
}