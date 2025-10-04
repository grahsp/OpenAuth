using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record SecretInfo(
    SecretId Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt
);