using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Jwks;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.Security.Jwks;

public class RsaPublicKeyExporter : IPublicKeyExporter
{
    public KeyType KeyType => KeyType.RSA;
    
    public KeyParameters Export(Key key)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(key.Value);

        var parameters = rsa.ExportParameters(false);
        if (parameters.Modulus is null || parameters.Exponent is null)
            throw new InvalidOperationException("RSA parameters are invalid: missing modulus or exponent.");
 
        return new KeyParameters(
            N: Base64UrlEncoder.Encode(parameters.Modulus),
            E: Base64UrlEncoder.Encode(parameters.Exponent)
        );
    }
}