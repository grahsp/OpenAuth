using OpenAuth.Api.Dtos;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Api.Mappers;

public static class SigningKeyMapper
{
    public static SigningKeyResponse ToResponse(SigningKey key)
        => new SigningKeyResponse(
            key.KeyId.Value,
            key.Algorithm.ToString(),
            key.IsActive(),
            key.CreatedAt,
            key.ExpiresAt,
            key.RevokedAt
        );
}