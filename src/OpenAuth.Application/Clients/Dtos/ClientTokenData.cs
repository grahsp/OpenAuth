using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Dtos;

public record ClientTokenData(
    ClientId Id,
    IEnumerable<Audience> AllowedAudiences,
    IEnumerable<GrantType> AllowedGrantTypes,
    TimeSpan TokenLifetime
);