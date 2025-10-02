namespace OpenAuth.Domain.ValueObjects;

public record SecretCreationResult
(
    string PlainTextSecret,
    string SecretId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt
);