using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Oidc;

public sealed record IdTokenContext(ClientTokenData TokenData, ScopeCollection OidcScopes, AuthorizationGrant AuthorizationGrant);