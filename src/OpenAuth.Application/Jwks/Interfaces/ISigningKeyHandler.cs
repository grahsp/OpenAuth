using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Jwks.Dtos;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Application.Jwks.Interfaces;

public interface ISigningKeyHandler
{
    KeyType KeyType { get; }
    BaseJwk CreateJwk(SigningKey signingKey);
    SigningCredentials CreateSigningCredentials(SigningKey signingKey);
    SecurityKey CreateValidationKey(SigningKey signingKey);
}