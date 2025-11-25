using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public record OidcContext(
    string Nonce,
    int AuthTimeInSeconds,
    ScopeCollection Scopes
);