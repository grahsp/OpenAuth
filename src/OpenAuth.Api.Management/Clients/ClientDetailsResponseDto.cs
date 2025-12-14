namespace OpenAuth.Api.Management.Clients;

public sealed record ClientDetailsResponseDto(
    string Id,
    string Name,
    IEnumerable<AudienceResponseDto> Audiences,
    IEnumerable<string> RedirectUris,
    IEnumerable<string> GrantTypes,
    bool Enabled,
    DateTimeOffset CreatedAt
);