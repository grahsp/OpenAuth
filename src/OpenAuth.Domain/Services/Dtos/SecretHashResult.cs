using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Domain.Services.Dtos;

public sealed record SecretHashResult(SecretHash Hash, string PlainTextSecret);