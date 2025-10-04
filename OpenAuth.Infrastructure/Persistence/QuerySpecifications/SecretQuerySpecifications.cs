using OpenAuth.Application.Dtos;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Infrastructure.Persistence.QuerySpecifications;

public static class SecretQuerySpecifications
{
    public static IQueryable<ClientSecret> WhereActive(
        this IQueryable<ClientSecret> query,
        DateTimeOffset now)
        => query.Where(k => k.RevokedAt == null && k.ExpiresAt > now);

    public static IQueryable<SecretInfo> ToSecretInfo(this IQueryable<ClientSecret> query)
        => query.Select(s => new SecretInfo(
            s.Id,
            s.CreatedAt,
            s.ExpiresAt,
            s.RevokedAt
        ));
}