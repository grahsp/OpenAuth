using OpenAuth.Api.Dtos;
using OpenAuth.Application.Dtos;

namespace OpenAuth.Api.Mappers;

public static class ClientMapper
{
    public static ClientResponse ToResponse(ClientInfo client)
        => new ClientResponse(
            client.Id.Value.ToString(),
            client.Name.Value,
            [],
            [],
            client.CreatedAt,
            client.UpdatedAt
        );

    public static ClientResponse ToResponse(ClientDetails client)
        => new ClientResponse(
            client.Id.Value.ToString(),
            client.Name.Value,
            client.Audiences.Select(ToResponse),
            client.Secrets.Select(ToResponse),
            client.CreatedAt,
            client.UpdatedAt
        );
    
     public static AudienceResponse ToResponse(AudienceInfo audience)
        => new AudienceResponse(
            audience.Name.Value,
            audience.Scopes.Select(x => x.Value)
        );

    public static SecretResponse ToResponse(SecretInfo secret)
        => new SecretResponse(
            secret.Id.Value,
            Guid.NewGuid(),
            secret.CreatedAt,
            secret.ExpiresAt,
            secret.RevokedAt
        );
}