using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Oidc;

public sealed record IdTokenContext(Client Client, ScopeCollection OidcScopes, AuthorizationGrant AuthorizationGrant);