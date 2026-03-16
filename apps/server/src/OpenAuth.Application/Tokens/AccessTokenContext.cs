using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens;

public sealed record AccessTokenContext(
    string? ClientId,
    string? Audience,
    string? Subject,
    int LifetimeInSeconds,
    ScopeCollection Scope
);