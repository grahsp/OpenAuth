using OpenAuth.Application.Audiences.Dtos;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Services;

namespace OpenAuth.Api.Management.Clients;

public static class ClientResponseMapper
{
    public static CreateClientRequestDto ToResponse(this CreateClientResult result)
        => new(result.Client.Id.ToString(), result.ClientSecret);

    public static AudienceResponseDto ToResponse(this AudienceInfo audience)
        => new(audience.Name.Value, audience.Scopes.Select(s => s.Value));

    public static ClientDetailsResponseDto ToResponse(this ClientDetailsResult client)
        => new(
            client.Id.ToString(),
            client.Name.Value,
            client.Audiences.Select(a => a.ToResponse()),
            client.RedirectUris.Select(r => r.Value),
            client.GrantTypes.Select(g => g.Value),
            true,
            client.CreatedAt
        );
}