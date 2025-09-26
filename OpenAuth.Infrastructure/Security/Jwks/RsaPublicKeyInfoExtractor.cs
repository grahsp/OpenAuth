using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Jwks;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.Security.Jwks;

public class RsaPublicKeyInfoExtractor : IPublicKeyInfoExtractor
{
    public KeyType KeyType => KeyType.RSA;
    
    public PublicKeyInfo Extract(SigningKeyId kid, KeyMaterial keyMaterial)
    {
        // TODO: Add caching in future
        using var rsa = RSA.Create();
        rsa.ImportFromPem(keyMaterial.Key.Value);

        var export = rsa.ExportParameters(false);
        if (export.Modulus is null || export.Exponent is null)
            throw new InvalidOperationException(
                $"RSA key '{ kid }' is invalid: missing modulus or exponent.");

        var n = Base64UrlEncoder.Encode(export.Modulus);
        var e = Base64UrlEncoder.Encode(export.Exponent);
        
        var publicKeyInfo = new RsaPublicKeyInfo(kid, keyMaterial.Alg, n, e);
        
        return publicKeyInfo;
    }
}