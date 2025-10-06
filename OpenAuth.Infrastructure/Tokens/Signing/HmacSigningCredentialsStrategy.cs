using System.Text;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Dtos;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Extensions;

namespace OpenAuth.Infrastructure.Tokens.SigningCredentials;

/// <summary>
/// Provides signing credentials extraction for HMAC key material.
/// Wraps a symmetric secret in a <see cref="SymmetricSecurityKey"/> 
/// and produces <see cref="SigningCredentials"/> suitable for JWT signing.
/// </summary>
public class HmacSigningCredentialsStrategy : ISigningCredentialsStrategy
{
    /// <inheritdoc />
    public KeyType KeyType => KeyType.HMAC;

    /// <inheritdoc />
    public Microsoft.IdentityModel.Tokens.SigningCredentials GetSigningCredentials(SigningKeyData keyData)
    {
        ArgumentNullException.ThrowIfNull(keyData);
        
        if (keyData.Kty != KeyType)
            throw new InvalidOperationException(
                $"Expected { KeyType } key material but got { keyData.Kty } (alg: { keyData.Alg }).");
        
        var bytes= Encoding.UTF8.GetBytes(keyData.Key.Value);
        var securityKey = new SymmetricSecurityKey(bytes) { KeyId = keyData.Kid.Value.ToString() };
        
        return new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, keyData.Alg.ToSecurityString());
    }
}