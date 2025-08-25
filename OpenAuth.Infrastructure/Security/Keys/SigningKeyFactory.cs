using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Infrastructure.Security.Keys;

public class SigningKeyFactory : ISigningKeyFactory
{
    private readonly Dictionary<SigningAlgorithm, ISigningKeyStrategy> _strategies;
    
    public SigningKeyFactory(IEnumerable<ISigningKeyStrategy> factories)
    {
        _strategies = factories.ToDictionary(s => s.Algorithm);
    }

    public SigningKey Create(SigningAlgorithm algorithm, DateTime? expiresAt = null)
    {
        if (!_strategies.TryGetValue(algorithm, out var strategy))
            throw new InvalidOperationException($"Unsupported algorithm used { algorithm }.");

        return strategy.Create(expiresAt);
    }
}