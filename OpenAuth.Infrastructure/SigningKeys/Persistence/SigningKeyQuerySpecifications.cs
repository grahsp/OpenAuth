using OpenAuth.Domain.SigningKeys;

namespace OpenAuth.Infrastructure.SigningKeys.Persistence;

public static class SigningKeyQuerySpecifications
{
    public static IQueryable<SigningKey> WhereActive(
        this IQueryable<SigningKey> query,
        DateTimeOffset now)
        => query.Where(k => k.RevokedAt == null && k.ExpiresAt > now);
}