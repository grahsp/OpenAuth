namespace OpenAuth.Api.Dtos;

public record SigningKeySummaryResponse(
    Guid Id,
    string Algorithm,
    DateTime CreatedAt,
    DateTime? ExpiresAt
);