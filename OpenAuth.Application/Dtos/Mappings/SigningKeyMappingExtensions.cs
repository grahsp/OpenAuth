using OpenAuth.Domain.SigningKeys;

namespace OpenAuth.Application.Dtos.Mappings;

public static class SigningKeyMappingExtensions
{
    public static SigningKeyInfo ToSigningKeyInfo(this SigningKey key)
        => new SigningKeyInfo(
            key.Id,
            key.KeyMaterial.Kty,
            key.KeyMaterial.Alg,
            key.CreatedAt,
            key.ExpiresAt,
            key.RevokedAt
        );
}