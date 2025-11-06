using OpenAuth.Domain.Clients.Audiences;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Dtos;

public record ClientAuthorizationData(
    ClientId ClientId,
    bool RequirePkce,
    GrantType[] GrantTypes,
    RedirectUri[] RedirectUris,
    NewAudience[] AllowedAudiences
);