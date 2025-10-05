using OpenAuth.Application.Dtos;
using OpenAuth.Application.Security.Jwks;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Infrastructure.Security.Jwks;

public class PublicKeyInfoFactory : IPublicKeyInfoFactory
{
    private readonly Dictionary<KeyType, IPublicKeyInfoExtractor> _extractors;
    
    public PublicKeyInfoFactory(IEnumerable<IPublicKeyInfoExtractor> extractors)
    {
        _extractors = extractors
            .ToDictionary(e => e.KeyType);
    }
    
    
    public PublicKeyInfo Create(SigningKeyData keyData)
    {
        if (!_extractors.TryGetValue(keyData.Kty, out var exporter))
            throw new InvalidOperationException(
                $"No JWK factory registered for key type '{ keyData.Kty }'. kid: { keyData.Kid }."
            );
        
        return exporter.Extract(keyData);
    }
}