using OpenAuth.Domain.Entities;

namespace OpenAuth.Domain.ValueObjects;

public record SecretCreationResult(ClientSecret Secret, string Plain);