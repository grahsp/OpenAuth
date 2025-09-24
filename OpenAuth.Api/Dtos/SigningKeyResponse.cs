namespace OpenAuth.Api.Dtos;

public record SigningKeyResponse(
    Guid KeyId,
    string Algorithm,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    DateTime? RevokedAt
);