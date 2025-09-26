using OpenAuth.Application.Security.Jwks;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.Security.Jwks;

public class PublicKeyInfoFactory : IPublicKeyInfoFactory
{
    private readonly Dictionary<KeyType, IPublicKeyInfoExtractor> _extractors;
    
    public PublicKeyInfoFactory(IEnumerable<IPublicKeyInfoExtractor> extractors)
    {
        _extractors = extractors
            .ToDictionary(e => e.KeyType);
    }
    
    
    public PublicKeyInfo Create(SigningKeyId kid, KeyMaterial keyMaterial)
    {
        if (!_extractors.TryGetValue(keyMaterial.Kty, out var exporter))
            throw new InvalidOperationException(
                $"No JWK factory registered for key type '{ keyMaterial.Kty }'. kid: { kid }."
            );
        
        return exporter.Extract(kid, keyMaterial);
    }
}