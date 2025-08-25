using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Application.Tests.Stubs;

public class FakeSigningCredentialsFactory : ISigningCredentialsFactory
{
    public SigningCredentials Create(SigningKey signingKey)
    {
        var bytes = Convert.FromBase64String(signingKey.PrivateKey);
        var key = new SymmetricSecurityKey(bytes) { KeyId = signingKey.KeyId.ToString() };
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }
}