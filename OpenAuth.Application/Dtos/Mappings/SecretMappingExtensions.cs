using OpenAuth.Domain.Clients.Secrets;

namespace OpenAuth.Application.Dtos.Mappings;

public static class SecretMappingExtensions
{
    public static SecretInfo ToSecretInfo(this ClientSecret secret)
        => new SecretInfo(
            secret.Id,
            secret.CreatedAt,
            secret.ExpiresAt,
            secret.RevokedAt
        );
}