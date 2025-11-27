using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public sealed record AuthorizationValidationResult(
    ClientId ClientId,
    ScopeCollection Scopes,
    RedirectUri RedirectUri,
    Pkce? Pkce,
    string? Nonce
);