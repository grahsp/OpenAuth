using OpenAuth.Api.Dtos;
using OpenAuth.Application.Dtos;

namespace OpenAuth.Api.Mappers;

public static class SigningKeyMapper
{
    public static SigningKeyResponse ToResponse(SigningKeyInfo key)
        => new(
            key.Id.Value,
            key.Algorithm.ToString(),
            key.CreatedAt,
            key.ExpiresAt,
            key.RevokedAt
        );
}