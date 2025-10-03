using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record ClientSecretDto(
    SecretId Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt
);