using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Security.Jwks;

public interface IPublicKeyInfoExtractor
{
    KeyType KeyType { get; }
    PublicKeyInfo Extract(SigningKeyId kid, KeyMaterial keyMaterial);
}