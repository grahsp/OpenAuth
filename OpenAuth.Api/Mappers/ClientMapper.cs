using OpenAuth.Api.Dtos;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Api.Mappers;

public static class ClientMapper
{
    public static ClientResponse ToResponse(Client client)
        => new ClientResponse(
            client.Id.ToString(),
            client.Name.ToString(),
            client.Enabled,
            client.Audiences.Select(ToResponse),
            client.Secrets.Select(x => new ClientSecretSummaryResponse(x.Id.Value, x.CreatedAt, x.ExpiresAt))
        );

    public static AudienceResponse ToResponse(Audience audience)
        => new AudienceResponse(
            audience.Value,
            audience.Scopes.Select(x => x.Value)
        );

    public static ClientSecretResponse ToResponse(ClientSecret secret)
        => new ClientSecretResponse(
            secret.Id.Value,
            secret.ClientId.Value,
            secret.CreatedAt,
            secret.ExpiresAt,
            secret.RevokedAt
        );
}