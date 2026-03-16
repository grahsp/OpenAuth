using OpenAuth.Application.Clients.Mappings;

namespace OpenAuth.Api.Dtos;

public record ClientResponse(
    string Id,
    string Name,
    IEnumerable<ClientApiAccessDetails> ApiAccess,
    IEnumerable<SecretResponse> Secrets,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);