using OpenAuth.Application.Jwks.Dtos;
using OpenAuth.Application.SigningKeys.Dtos;

namespace OpenAuth.Application.Jwks.Interfaces;

public interface IPublicKeyInfoFactory
{
    PublicKeyInfo Create(SigningKeyData keyData);
}