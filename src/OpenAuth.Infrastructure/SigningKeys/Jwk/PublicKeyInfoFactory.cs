using OpenAuth.Application.Jwks.Dtos;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Infrastructure.SigningKeys.Jwk;

public class PublicKeyInfoFactory : IPublicKeyInfoFactory
{
    private readonly Dictionary<KeyType, ISigningKeyHandler> _extractors;
    
    public PublicKeyInfoFactory(IEnumerable<ISigningKeyHandler> extractors)
    {
        _extractors = extractors.ToDictionary(e => e.KeyType);
    }
    
    public PublicKeyInfo Create(SigningKey signingKey)
    {
        if (!_extractors.TryGetValue(signingKey.KeyMaterial.Kty, out var exporter))
            throw new InvalidOperationException($"No JWK factory registered for key type '{ signingKey.KeyMaterial.Kty }'. kid: { signingKey.Id }.");
        
        return exporter.CreateJwk(signingKey);
    }
}