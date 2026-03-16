using OpenAuth.Domain.Clients.Secrets;

namespace OpenAuth.Infrastructure.Clients.Secrets.Persistence;

public static class SecretQuerySpecifications
{
    public static IQueryable<Secret> WhereActive(
        this IQueryable<Secret> query,
        DateTimeOffset now)
        => query.Where(k => k.RevokedAt == null && k.ExpiresAt > now);
}