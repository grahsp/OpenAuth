using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.ManagementApi.Dtos;

namespace OpenAuth.ManagementApi.Mappers;

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