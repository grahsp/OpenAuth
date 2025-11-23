namespace OpenAuth.Api.Dtos;

public record ClientResponse(
    string Id,
    string Name,
    IEnumerable<AudienceResponse> Audiences,
    IEnumerable<SecretResponse> Secrets,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);