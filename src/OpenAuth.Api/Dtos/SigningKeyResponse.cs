namespace OpenAuth.Api.Dtos;

public record SigningKeyResponse(
    Guid KeyId,
    string Algorithm,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt
);