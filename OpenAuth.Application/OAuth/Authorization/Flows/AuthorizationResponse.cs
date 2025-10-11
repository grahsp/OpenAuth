using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Flows;

public record AuthorizationResponse
(
    string Code,
    RedirectUri RedirectUri
);