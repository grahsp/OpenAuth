namespace OpenAuth.Server.SigningKeys;

public record SigningKeyResponse(
    Guid KeyId,
    string Algorithm,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt
);