using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Dtos;

public record ClientTokenData(
    ScopeCollection Scopes,
    IEnumerable<GrantType> AllowedGrantTypes,
    TimeSpan TokenLifetime
);