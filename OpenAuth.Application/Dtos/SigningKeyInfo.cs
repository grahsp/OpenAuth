using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record SigningKeyInfo(
    SigningKeyId Id,
    KeyType KeyType,
    SigningAlgorithm Algorithm,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt
);