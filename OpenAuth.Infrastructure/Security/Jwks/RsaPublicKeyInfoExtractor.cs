using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Dtos;
using OpenAuth.Application.Security.Jwks;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Infrastructure.Security.Jwks;

public class RsaPublicKeyInfoExtractor : IPublicKeyInfoExtractor
{
    public KeyType KeyType => KeyType.RSA;
    
    public PublicKeyInfo Extract(SigningKeyData keyData)
    {
        // TODO: Add caching in future
        using var rsa = RSA.Create();
        rsa.ImportFromPem(keyData.Key.Value);

        var export = rsa.ExportParameters(false);
        if (export.Modulus is null || export.Exponent is null)
            throw new InvalidOperationException(
                $"RSA key '{ keyData.Kid }' is invalid: missing modulus or exponent.");

        var n = Base64UrlEncoder.Encode(export.Modulus);
        var e = Base64UrlEncoder.Encode(export.Exponent);
        
        var publicKeyInfo = new RsaPublicKeyInfo(keyData.Kid, keyData.Alg, n, e);
        
        return publicKeyInfo;
    }
}