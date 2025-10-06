using OpenAuth.Application.Dtos;
using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Application.Security.Jwks;

public interface IPublicKeyInfoExtractor
{
    KeyType KeyType { get; }
    PublicKeyInfo Extract(SigningKeyData keyData);
}