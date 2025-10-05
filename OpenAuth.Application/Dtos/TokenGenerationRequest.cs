using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record TokenGenerationRequest(
    ClientId ClientId,
    AudienceName AudienceName,
    Scope[] Scopes,
    TimeSpan TokenLifetime,
    SigningKeyData KeyData
);