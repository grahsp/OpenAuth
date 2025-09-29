using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.Extensions;

namespace OpenAuth.Infrastructure.Security.Keys;

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

    public SigningKey Create(SigningAlgorithm algorithm, DateTime createdAt, TimeSpan? lifetime = null)
    {
        if (createdAt == default)
            throw new ArgumentException("Creation date must be explicitly set.", nameof(createdAt));

        if (createdAt.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Creation date must be expressed in UTC.", nameof(createdAt));
        
        if (lifetime.HasValue && lifetime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Lifetime must be positive.");
        
        if (!_generators.TryGetValue(algorithm.ToKeyType(), out var generator))
            throw new InvalidOperationException($"Unsupported algorithm used { algorithm }.");

        var keyMaterial = generator.Create(algorithm);
        var expiresAt = createdAt.Add(lifetime ?? TimeSpan.FromDays(DefaultLifetimeInDays));
        
        return new SigningKey(keyMaterial, createdAt, expiresAt);
    }
}