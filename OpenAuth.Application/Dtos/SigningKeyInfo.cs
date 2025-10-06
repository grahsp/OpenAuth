using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record SigningKeyInfo(
    SigningKeyId Id,
    KeyType KeyType,
    SigningAlgorithm Algorithm,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt
);