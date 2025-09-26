using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.Extensions;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Security.Extensions;

namespace OpenAuth.Infrastructure.Security.Signing;

public class RsaSigningCredentialsStrategy : ISigningCredentialsStrategy
{
    public KeyType KeyType => KeyType.RSA;

    public SigningCredentials GetSigningCredentials(string kid, KeyMaterial keyMaterial)
    {
        if (keyMaterial.Alg.ToKeyType() != KeyType)
            throw new InvalidOperationException(
                $"Expected { KeyType } key but got { keyMaterial.Kty } (alg: { keyMaterial.Alg }).");
        
        using var privateRsa = RSA.Create();
        privateRsa.ImportFromPem(keyMaterial.Key.Value);
        var securityKey = new RsaSecurityKey(privateRsa) { KeyId = kid };
        
        return new SigningCredentials(securityKey,keyMaterial.Alg.ToSecurityString());
    }
}