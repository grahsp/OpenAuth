using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Domain.Services.Dtos;

public record SecretCreationResult
(
    SecretId SecretId,
    string PlainTextSecret,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt
);