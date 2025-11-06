using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public record TokenContext(
    ClientId ClientId,
    string Subject,
    AudienceName RequestedAudience,
    ScopeCollection RequestedScopes
);