namespace OpenAuth.Api.Dtos;

public record ClientResponse(
    Guid Id,
    string Name,
    bool IsActive,
    IEnumerable<AudienceResponse> Audiences,
    IEnumerable<ClientSecretSummaryResponse> Secrets
);