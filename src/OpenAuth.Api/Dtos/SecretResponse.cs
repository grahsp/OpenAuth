namespace OpenAuth.Api.Dtos;

public record SecretResponse(
    Guid Id,
    Guid ClientId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt
);