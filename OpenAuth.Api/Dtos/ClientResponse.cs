namespace OpenAuth.Api.Dtos;

public record ClientResponse(
    Guid Id,
    string Name,
    bool IsActive,
    ClientSecretSummaryResponse[] Secrets,
    SigningKeySummaryResponse[] SigningKeys
);