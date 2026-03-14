using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public sealed record TokenContext(
    ScopeCollection Scope,
    string? ClientId,
    AudienceIdentifier? Audience = null,
    string? Subject = null,
    OidcContext? OidcContext = null
);