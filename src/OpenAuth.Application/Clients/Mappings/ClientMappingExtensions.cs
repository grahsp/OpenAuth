using OpenAuth.Application.Audiences.Mappings;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.Clients;

namespace OpenAuth.Application.Clients.Mappings;

public static class ClientMappingExtensions
{
    public static ClientInfo ToClientInfo(this Client client)
        => new(
            client.Id.ToString(),
            client.Name.ToString(),
            client.ApplicationType.Name,
            client.Enabled,
            client.CreatedAt
        );

    public static ClientDetailsResult ToClientDetails(this Client client)
        => new(
            client.Id,
            client.Name,
            [..client.AllowedAudiences.Select(a => a.ToAudienceInfo())],
            client.RedirectUris,
            client.AllowedGrantTypes,
            client.Enabled,
            client.CreatedAt
        );
}