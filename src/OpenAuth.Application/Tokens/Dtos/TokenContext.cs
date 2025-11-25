using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public record TokenContext(
    string ClientId,
    string? Subject,
    string Audience,
    ScopeCollection Scopes,
    OidcContext? OidcContext = null
);