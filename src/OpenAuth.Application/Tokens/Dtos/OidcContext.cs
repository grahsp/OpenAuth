using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public record OidcContext(
    int AuthTimeInSeconds,
    ScopeCollection Scopes,
    string? Nonce = null
);