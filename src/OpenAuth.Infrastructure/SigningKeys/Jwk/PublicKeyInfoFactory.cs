using OpenAuth.Application.Jwks;
using OpenAuth.Application.Jwks.Dtos;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Infrastructure.SigningKeys.Jwk;

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