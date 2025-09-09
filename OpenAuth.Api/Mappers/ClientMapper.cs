using OpenAuth.Api.Dtos;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Api.Mappers;

public static class ClientMapper
{
    public static ClientResponse ToResponse(Client client)
        => new ClientResponse(
            client.Id.Value,
            client.Name,
            client.Enabled,
            client.Secrets.Select(x => new ClientSecretSummaryResponse(x.Id.Value, x.CreatedAt, x.ExpiresAt)).ToArray(),
            client.SigningKeys
                .Select(x => new SigningKeySummaryResponse(x.KeyId.Value, x.Algorithm.ToString(), x.CreatedAt, x.ExpiresAt))
                .ToArray()
        );
}