using OpenAuth.Domain.Clients.ApplicationType;

namespace OpenAuth.Domain.Clients.ValueObjects;

public sealed record ClientConfiguration(
    ClientName Name,
    ClientApplicationType ApplicationType,
    IEnumerable<Audience> AllowedAudiences,
    IEnumerable<GrantType> AllowedGrantTypes,
    IEnumerable<RedirectUri> RedirectUris
);