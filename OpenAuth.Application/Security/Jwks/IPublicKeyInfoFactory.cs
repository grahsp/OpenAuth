using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Security.Jwks;

public interface IPublicKeyInfoFactory
{
    PublicKeyInfo Create(SigningKeyId kid, KeyMaterial keyMaterial);
}