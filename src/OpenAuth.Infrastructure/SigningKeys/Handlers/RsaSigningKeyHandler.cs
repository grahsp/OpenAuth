using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Jwks.Dtos;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Infrastructure.Security.Extensions;

namespace OpenAuth.Infrastructure.SigningKeys.Handlers;

public class RsaSigningKeyHandler : ISigningKeyHandler
{
    public KeyType KeyType => KeyType.RSA;
    
    public PublicKeyInfo CreateJwk(SigningKey signingKey)
    {
        var material = ValidateAndExtractKeyMaterial(signingKey);
        var (n, e) = ExtractPublicParameters(signingKey);
        
        var publicKeyInfo = new RsaPublicKeyInfo(signingKey.Id, material.Alg, n, e);
        return publicKeyInfo;
    }

    public SigningCredentials CreateSigningCredentials(SigningKey signingKey)
    {
        var material = ValidateAndExtractKeyMaterial(signingKey);
        
        var privateRsa = RSA.Create();
        privateRsa.ImportFromPem(material.Key.Value);
        var securityKey = new RsaSecurityKey(privateRsa) { KeyId = signingKey.Id.ToString() };
        
        return new SigningCredentials(securityKey, material.Alg.ToSecurityString());
    }

    public SecurityKey CreateValidationKey(SigningKey signingKey)
    {
        ValidateAndExtractKeyMaterial(signingKey);
        var (n, e) = ExtractPublicParameters(signingKey);

        var rsaParams = new RSAParameters
        {
            Modulus = Base64UrlEncoder.DecodeBytes(n),
            Exponent = Base64UrlEncoder.DecodeBytes(e)
        };
        
        var rsa = new RsaSecurityKey(rsaParams)
        {
            KeyId = signingKey.Id.ToString()
        };

        return rsa;
    }

    public static (string N, string E) ExtractPublicParameters(SigningKey signingKey)
    {
        // TODO: cache modulus/exponent per keyId since RSA public parameters never change.

        using var rsa = RSA.Create();
        rsa.ImportFromPem(signingKey.KeyMaterial.Key.Value);
        
        var rsaParams = rsa.ExportParameters(false);
        var n = Base64UrlEncoder.Encode(rsaParams.Modulus);
        var e = Base64UrlEncoder.Encode(rsaParams.Exponent);
        
        if (rsaParams.Modulus is null || rsaParams.Exponent is null)
            throw new InvalidOperationException($"RSA key '{ signingKey.Id.Value }' is invalid: missing modulus or exponent.");

        return (n, e);
    }

    private KeyMaterial ValidateAndExtractKeyMaterial(SigningKey signingKey)
    {
        ArgumentNullException.ThrowIfNull(signingKey);

        var material = signingKey.KeyMaterial;
        if (material.Kty != KeyType)
            throw new InvalidOperationException($"Handler '{GetType().Name}' cannot process key type '{material.Kty}'. Expected '{KeyType}'.");
        
        return material;
    }
}