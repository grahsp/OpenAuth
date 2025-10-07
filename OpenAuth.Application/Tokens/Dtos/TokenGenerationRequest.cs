using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public record TokenGenerationRequest(
    ClientId ClientId,
    AudienceName AudienceName,
    Scope[] Scopes,
    TimeSpan TokenLifetime,
    SigningKeyData KeyData
);