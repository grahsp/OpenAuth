using OpenAuth.Api.Dtos;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Api.Mappers;

public static class ClientMapper
{
    public static ClientResponse ToResponse(Client client)
        => new ClientResponse(
            client.Id.Value,
            client.Name,
            client.Enabled,
            client.Audiences.Select(ToResponse),
            client.Secrets.Select(x => new ClientSecretSummaryResponse(x.Id.Value, x.CreatedAt, x.ExpiresAt)),
            client.SigningKeys
                .Select(x => new SigningKeySummaryResponse(x.KeyId.Value, x.Algorithm.ToString(), x.CreatedAt, x.ExpiresAt))
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
            secret.IsActive(),
            secret.CreatedAt,
            secret.ExpiresAt,
            secret.RevokedAt
        );

    public static SigningKeyResponse ToResponse(SigningKey key)
        => new SigningKeyResponse(
            key.KeyId.Value,
            key.ClientId.Value,
            key.Algorithm.ToString(),
            key.IsActive(),
            key.CreatedAt,
            key.ExpiresAt,
            key.RevokedAt
        );
}