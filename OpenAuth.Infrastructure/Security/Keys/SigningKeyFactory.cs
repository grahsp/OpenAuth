using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Infrastructure.Security.Keys;

public class SigningKeyFactory : ISigningKeyFactory
{
    private readonly Dictionary<SigningAlgorithm, IKeyMaterialGenerator> _generators;
    public const int DefaultLifetimeInDays = 30;
    
    public SigningKeyFactory(IEnumerable<IKeyMaterialGenerator> generators)
    {
        try
        {
            _generators = generators
                .SelectMany(g => g.SupportedAlgorithms.Select(a => (algorithm: a, generator: g)))
                .ToDictionary(x => x.algorithm, x => x.generator);
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
            throw new ArgumentException("Creation date must be a valid UTC timestamp.", nameof(createdAt));
        
        if (lifetime.HasValue && lifetime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Lifetime must be positive.");
        
        if (!_generators.TryGetValue(algorithm, out var generator))
            throw new InvalidOperationException($"Unsupported algorithm used { algorithm }.");

        var keyMaterial = generator.Create(algorithm);
        var expiresAt = createdAt.Add(lifetime ?? TimeSpan.FromDays(DefaultLifetimeInDays));
        
        return new SigningKey(keyMaterial, createdAt, expiresAt);
    }
}