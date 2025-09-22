using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Api.Controllers;
using OpenAuth.Api.Dtos;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Api.Mappers;

public static class SigningKeyMapper
{
    public static SigningKeyResponse ToResponse(SigningKey key)
        => new SigningKeyResponse(
            key.KeyId.Value,
            key.Algorithm.ToString(),
            key.IsActive(),
            key.CreatedAt,
            key.ExpiresAt,
            key.RevokedAt
        );

    public static Jwk ToJwk(SigningKey key)
    {
        var (n, e) = ExportRsaParameters(key.PrivateKey);
        
        return new Jwk
        {
            Kid = key.KeyId.Value.ToString(),
            Kty = "RSA",
            Alg = "RS256",
            Use = "sig",
            N = n,
            E = e,
        };
    
        (string N, string E) ExportRsaParameters(string privateKeyPem)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);

            var parameters = rsa.ExportParameters(false);
            if (parameters.Modulus is null || parameters.Exponent is null)
                throw new InvalidOperationException("RSA parameters are invalid: missing modulus or exponent.");
 
            return (
                N: Base64UrlEncoder.Encode(parameters.Modulus),
                E: Base64UrlEncoder.Encode(parameters.Exponent)
            );
        }
    }
}