using OpenAuth.Application.Audiences.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Dtos;

public record ClientDetailsResult(
    ClientId Id,
    ClientName Name,
    IReadOnlyCollection<AudienceInfo> Audiences,
    IReadOnlyCollection<RedirectUri> RedirectUris,
    IReadOnlyCollection<GrantType> GrantTypes,
    bool Enabled,
    DateTimeOffset CreatedAt
);