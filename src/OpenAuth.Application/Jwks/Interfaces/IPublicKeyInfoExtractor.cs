using OpenAuth.Application.Jwks.Dtos;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Application.Jwks.Interfaces;

public interface IPublicKeyInfoExtractor
{
    KeyType KeyType { get; }
    PublicKeyInfo Extract(SigningKeyData keyData);
}