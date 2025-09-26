using OpenAuth.Application.Security.Jwks;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.Security.Jwks;

public class JwkFactory : IJwkFactory
{
    private readonly Dictionary<KeyType, IPublicKeyExporter> _exporters;
    
    public JwkFactory(IEnumerable<IPublicKeyExporter> exporters)
    {
        _exporters = exporters
            .ToDictionary(e => e.KeyType);
    }
    
    
    public Jwk Create(string kid, KeyMaterial keyMaterial)
    {
        if (!_exporters.TryGetValue(keyMaterial.Kty, out var exporter))
            throw new InvalidOperationException(
                $"Unable to create JWK: no parameter exporter registered for key type '{ keyMaterial.Kty }' (kid: { kid }).");

        var parameters = exporter.Export(keyMaterial.Key);

        return new Jwk
        {
            Kid = kid,
            Kty = keyMaterial.Kty.ToString(),
            Alg = keyMaterial.Alg.ToString(),
            Use = "sig",
            N = parameters.N,
            E = parameters.E,
        };
    }
}