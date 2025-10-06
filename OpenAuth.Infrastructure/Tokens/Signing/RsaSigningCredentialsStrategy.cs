using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Dtos;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Security.Extensions;

namespace OpenAuth.Infrastructure.Tokens.SigningCredentials;

/// <summary>
/// Provides signing credentials extraction for RSA key material.
/// Wraps an <see cref="RSA"/> private key in a <see cref="RsaSecurityKey"/> 
/// and produces <see cref="SigningCredentials"/> suitable for JWT signing.
/// </summary>
public class RsaSigningCredentialsStrategy : ISigningCredentialsStrategy
{
    /// <inheritdoc />
    public KeyType KeyType => KeyType.RSA;

    /// <inheritdoc />
    public Microsoft.IdentityModel.Tokens.SigningCredentials GetSigningCredentials(SigningKeyData keyData)
    {
        ArgumentNullException.ThrowIfNull(keyData);
        
        if (keyData.Kty != KeyType)
            throw new InvalidOperationException(
                $"Expected { KeyType } key material but got { keyData.Kty } (alg: { keyData.Alg }).");
        
        var privateRsa = RSA.Create();
        privateRsa.ImportFromPem(keyData.Key.Value);
        var securityKey = new RsaSecurityKey(privateRsa) { KeyId = keyData.Kid.Value.ToString() };
        
        return new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, keyData.Alg.ToSecurityString());
    }
}