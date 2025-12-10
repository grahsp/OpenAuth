using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Application.Jwks.Interfaces;

public interface ISigningKeyHandler
{
    KeyType KeyType { get; }
    SigningCredentials CreateSigningCredentials(SigningKey signingKey);
}