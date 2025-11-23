using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Application.Secrets.Dtos;

public record SecretInfo(
    SecretId Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt
);