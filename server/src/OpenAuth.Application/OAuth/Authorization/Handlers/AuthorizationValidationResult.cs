using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public sealed record AuthorizationValidationResult(
    ClientId ClientId,
    AudienceIdentifier Audience,
    ScopeCollection Scopes,
    RedirectUri RedirectUri,
    Pkce? Pkce,
    string? Nonce
);