namespace OpenAuth.Api.Dtos;

public record ClientSecretResponse(
    Guid Id,
    Guid ClientId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt
);