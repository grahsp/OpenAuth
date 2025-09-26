using OpenAuth.Api.Dtos;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Api.Mappers;

public static class SigningKeyMapper
{
    public static SigningKeyResponse ToResponse(SigningKey key)
        => new SigningKeyResponse(
            key.Id.Value,
            key.KeyMaterial.Alg.ToString(),
            key.CreatedAt,
            key.ExpiresAt,
            key.RevokedAt
        );
}