using System.Text;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Test.Common.Fakes;

public class FakeHmacSigningCredentialsFactory : ISigningCredentialsFactory
{
    public SigningCredentials Create(SigningKey signingKey)
    {
        var bytes = Encoding.UTF8.GetBytes(signingKey.KeyMaterial.Key.Value);
        var key = new SymmetricSecurityKey(bytes) { KeyId = signingKey.Id.ToString() };
        
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }
}