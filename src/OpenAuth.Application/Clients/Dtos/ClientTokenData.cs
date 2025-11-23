using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Dtos;

public record ClientTokenData(
    IEnumerable<Audience> AllowedAudiences,
    IEnumerable<GrantType> AllowedGrantTypes,
    TimeSpan TokenLifetime
);