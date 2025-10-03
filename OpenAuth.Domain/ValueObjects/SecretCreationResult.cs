namespace OpenAuth.Domain.ValueObjects;

public record SecretCreationResult
(
    string PlainTextSecret,
    SecretId SecretId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt
);