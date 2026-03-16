using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Dtos;

public record ClientAuthorizationData(
    ClientId ClientId,
    bool IsClientPublic,
    GrantType[] GrantTypes,
    RedirectUri[] RedirectUris
);