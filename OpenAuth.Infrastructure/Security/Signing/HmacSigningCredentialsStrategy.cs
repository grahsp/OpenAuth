using System.Text;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Infrastructure.Security.Signing;

public class HmacSigningCredentialsStrategy : ISigningCredentialsStrategy
{
    public SigningAlgorithm Algorithm => SigningAlgorithm.Hmac;

    public SigningCredentials GetSigningCredentials(SigningKey signingKey)
    {
        ArgumentNullException.ThrowIfNull(signingKey);
        if (signingKey.Algorithm != Algorithm)
            throw new InvalidOperationException($"Expected HMAC key but got { signingKey.Algorithm }.");
        
        var bytes= Encoding.UTF8.GetBytes(signingKey.Key.Value);
        var key = new SymmetricSecurityKey(bytes) { KeyId = signingKey.Id.ToString() };
        
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }
}