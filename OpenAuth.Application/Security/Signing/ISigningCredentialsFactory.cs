using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Application.Security.Signing;

public interface ISigningCredentialsFactory
{
    SigningCredentials Create(SigningKey signingKey);
}