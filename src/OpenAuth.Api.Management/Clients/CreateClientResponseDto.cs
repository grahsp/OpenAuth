namespace OpenAuth.Api.Management.Clients;

public sealed record CreateClientResponseDto(
    string Type,
    string Name,
    IEnumerable<string> RedirectUris
);