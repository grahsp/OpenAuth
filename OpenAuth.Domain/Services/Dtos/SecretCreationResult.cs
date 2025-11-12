using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Domain.Services.Dtos;

public record SecretCreationResult
(
    string PlainSecret,
    SecretHash Hash
);