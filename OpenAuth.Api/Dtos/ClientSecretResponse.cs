namespace OpenAuth.Api.Dtos;

public record ClientSecretResponse(
    Guid Id,
    Guid ClientId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    DateTime? RevokedAt
);