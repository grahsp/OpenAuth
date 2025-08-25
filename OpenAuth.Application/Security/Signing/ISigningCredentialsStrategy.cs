using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Application.Security.Signing;

public interface ISigningCredentialsStrategy
{
    SigningAlgorithm Algorithm { get; }
    SigningCredentials GetSigningCredentials(SigningKey signingKey);
}