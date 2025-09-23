using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Test.Common.Fakes;

public class FakeSigningCredentialsFactory : ISigningCredentialsFactory
{
    public SigningCredentials Create(SigningKey signingKey)
    {
        var bytes = Convert.FromBase64String(signingKey.Key.Value);
        var key = new SymmetricSecurityKey(bytes) { KeyId = signingKey.Id.ToString() };
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }
}