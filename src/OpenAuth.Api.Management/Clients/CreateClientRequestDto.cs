namespace OpenAuth.Api.Management.Clients;

public sealed record CreateClientRequestDto(
    string Id,
    string? Secret
);