using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Infrastructure.Security.Signing;

public class RsaSigningCredentialsStrategy : ISigningCredentialsStrategy
{
    public SigningAlgorithm Algorithm => SigningAlgorithm.Rsa;

    public SigningCredentials GetSigningCredentials(SigningKey signingKey)
    {
        ArgumentNullException.ThrowIfNull(signingKey);
        if (signingKey.Algorithm != Algorithm)
            throw new InvalidOperationException($"Expected RSA key but got { signingKey.Algorithm}.");
        
        var privateRsa = RSA.Create();
        privateRsa.ImportFromPem(signingKey.PrivateKey);
        var key = new RsaSecurityKey(privateRsa) { KeyId = signingKey.KeyId.ToString() };
        
        return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
    }
}