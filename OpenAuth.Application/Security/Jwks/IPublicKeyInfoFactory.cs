using OpenAuth.Application.Dtos;

namespace OpenAuth.Application.Security.Jwks;

public interface IPublicKeyInfoFactory
{
    PublicKeyInfo Create(SigningKeyData keyData);
}