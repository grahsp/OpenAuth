using System.Text;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Infrastructure.Security.Extensions;

namespace OpenAuth.Infrastructure.SigningKeys.Handlers;

public class HmacSigningKeyHandler : ISigningKeyHandler
{
    public KeyType KeyType => KeyType.HMAC;

    public SigningCredentials CreateSigningCredentials(SigningKey signingKey)
    {
        var material = ValidateAndExtractKeyMaterial(signingKey);
        var bytes= Encoding.UTF8.GetBytes(material.Key.Value);
        var securityKey = new SymmetricSecurityKey(bytes) { KeyId = signingKey.Id.ToString() };
        
        return new SigningCredentials(securityKey, material.Alg.ToSecurityString());
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