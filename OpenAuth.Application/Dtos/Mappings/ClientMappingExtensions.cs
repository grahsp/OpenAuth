using OpenAuth.Domain.Entities;

namespace OpenAuth.Application.Dtos.Mappings;

public static class ClientMappingExtensions
{
    public static ClientInfo ToClientInfo(this Client client)
        => new ClientInfo(
            client.Id,
            client.Name,
            client.CreatedAt,
            client.UpdatedAt
        );

    public static ClientDetails ToClientDetails(this Client client)
        => new ClientDetails(
            client.Id,
            client.Name,
            client.CreatedAt,
            client.UpdatedAt,
            client.Secrets
                .Select(s => s.ToSecretInfo()),
            client.Audiences
                .Select(a => a.ToAudienceInfo())
        );
}