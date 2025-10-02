namespace OpenAuth.Api.Dtos;

public record ClientSecretSummaryResponse(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt
);