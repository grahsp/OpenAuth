using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public sealed record TokenContext(
    ScopeCollection Scopes,
    string? ClientId,
    string? Audience = null,
    string? Subject = null,
    OidcContext? OidcContext = null
);