using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record SecretCreationResult
(
    string PlainTextSecret,
    SecretId SecretId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt
);