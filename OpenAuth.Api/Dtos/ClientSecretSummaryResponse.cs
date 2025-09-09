namespace OpenAuth.Api.Dtos;

public record ClientSecretSummaryResponse(
    Guid Id,
    DateTime CreatedAt,
    DateTime? ExpiresAt
);