namespace OpenAuth.Api.Management.Clients;

public sealed record ClientSummaryResponseDto(
    string Id,
    string Name,
    string Type,
    bool Enabled,
    DateTimeOffset CreatedAt
);