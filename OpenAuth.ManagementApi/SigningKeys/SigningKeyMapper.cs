using OpenAuth.Application.SigningKeys.Dtos;

namespace OpenAuth.ManagementApi.SigningKeys;

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