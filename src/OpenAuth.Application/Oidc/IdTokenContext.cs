using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Oidc;

public sealed record IdTokenContext(
    string ClientId,
    string Subject,
    string? Nonce,
    int AuthTimeInSeconds,
    int LifetimeInSeconds,
    ScopeCollection Scopes
);