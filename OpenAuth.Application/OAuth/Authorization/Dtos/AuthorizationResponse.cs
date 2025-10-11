using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Dtos;

public record AuthorizationResponse
(
    string Code,
    RedirectUri RedirectUri
);