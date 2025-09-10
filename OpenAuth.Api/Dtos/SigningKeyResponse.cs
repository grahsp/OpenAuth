namespace OpenAuth.Api.Dtos;

public record SigningKeyResponse(
    Guid KeyId,
    Guid ClientId,
    string Algorithm,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    DateTime? RevokedAt
);