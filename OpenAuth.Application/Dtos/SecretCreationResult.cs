using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record SecretCreationResult
(
    string PlainTextSecret,
    SecretId SecretId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt
);