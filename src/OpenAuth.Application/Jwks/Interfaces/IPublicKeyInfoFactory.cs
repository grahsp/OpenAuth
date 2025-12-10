using OpenAuth.Application.Jwks.Dtos;
using OpenAuth.Domain.SigningKeys;

namespace OpenAuth.Application.Jwks.Interfaces;

public interface IPublicKeyInfoFactory
{
    PublicKeyInfo Create(SigningKey signingKey);
}