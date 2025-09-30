namespace OpenAuth.Api.Dtos;

public record ClientResponse(
    string Id,
    string Name,
    bool IsActive,
    IEnumerable<AudienceResponse> Audiences,
    IEnumerable<ClientSecretSummaryResponse> Secrets
);