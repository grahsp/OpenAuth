using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Dtos;

public record AuthorizationRequest
(
    ClientId ClientId,
    GrantType GrantType,
    RedirectUri RedirectUri,
    AudienceName Audience,
    Scope[] Scopes,
    string CodeChallenge,
    string CodeChallengeMethod
);