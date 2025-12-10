using OpenAuth.Application.Jwks.Dtos;
using OpenAuth.Domain.SigningKeys;

namespace OpenAuth.Application.Jwks.Interfaces;

public interface IJwkFactory
{
    BaseJwk Create(SigningKey signingKey);
}