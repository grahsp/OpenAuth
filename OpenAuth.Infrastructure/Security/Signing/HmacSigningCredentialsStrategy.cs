using System.Text;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Security.Extensions;

namespace OpenAuth.Infrastructure.Security.Signing;

public class HmacSigningCredentialsStrategy : ISigningCredentialsStrategy
{
    public KeyType KeyType => KeyType.HMAC;

    public SigningCredentials GetSigningCredentials(string kid, KeyMaterial keyMaterial)
    {
        if (keyMaterial.Kty != KeyType)
            throw new InvalidOperationException(
                $"Expected { KeyType } key material but got { keyMaterial.Kty } (alg: { keyMaterial.Alg }).");
        
        var bytes= Encoding.UTF8.GetBytes(keyMaterial.Key.Value);
        var securityKey = new SymmetricSecurityKey(bytes) { KeyId = kid };
        
        return new SigningCredentials(securityKey, keyMaterial.Alg.ToSecurityString());
    }
}