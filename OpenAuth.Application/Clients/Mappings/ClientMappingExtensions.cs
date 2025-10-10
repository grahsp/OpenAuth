using OpenAuth.Application.Audiences.Mappings;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Secrets.Mappings;
using OpenAuth.Domain.Clients;

namespace OpenAuth.Application.Clients.Mappings;

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
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => s.ToSecretInfo()),
            client.AllowedAudiences
                .Select(a => a.ToAudienceInfo())
        );
}