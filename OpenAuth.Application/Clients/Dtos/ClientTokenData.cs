using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Dtos;

public record ClientTokenData(
    IEnumerable<Scope> AllowedScopes,
    IEnumerable<GrantType> AllowedGrantTypes,
    bool RequirePkce,
    TimeSpan TokenLifetime
);