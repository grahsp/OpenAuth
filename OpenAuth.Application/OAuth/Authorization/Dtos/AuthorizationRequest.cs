using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Dtos;

public record AuthorizationRequest
(
    ClientId ClientId,
    RedirectUri RedirectUri,
    AudienceName Audience,
    Scope[] Scopes,
    Pkce? Pkce
);