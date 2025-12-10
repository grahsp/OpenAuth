using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Infrastructure.SigningKeys.Jwks;

public class JwkFactory : IJwkFactory
{
    private readonly Dictionary<KeyType, ISigningKeyHandler> _handlers;
    
    public JwkFactory(IEnumerable<ISigningKeyHandler> handlers)
    {
        _handlers = handlers.ToDictionary(e => e.KeyType);
    }
    
    public Application.Jwks.Dtos.BaseJwk Create(SigningKey signingKey)
    {
        if (!_handlers.TryGetValue(signingKey.KeyMaterial.Kty, out var exporter))
            throw new InvalidOperationException($"No JWK factory registered for key type '{ signingKey.KeyMaterial.Kty }'. kid: { signingKey.Id }.");
        
        return exporter.CreateJwk(signingKey);
    }
}