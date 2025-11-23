using OpenAuth.Application.Secrets.Dtos;
using OpenAuth.Domain.Clients.Secrets;

namespace OpenAuth.Application.Secrets.Mappings;

public static class SecretMappingExtensions
{
    public static SecretInfo ToSecretInfo(this Secret secret)
        => new SecretInfo(
            secret.Id,
            secret.CreatedAt,
            secret.ExpiresAt,
            secret.RevokedAt
        );
}