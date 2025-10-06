using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record TokenGenerationRequest(
    ClientId ClientId,
    AudienceName AudienceName,
    Scope[] Scopes,
    TimeSpan TokenLifetime,
    SigningKeyData KeyData
);