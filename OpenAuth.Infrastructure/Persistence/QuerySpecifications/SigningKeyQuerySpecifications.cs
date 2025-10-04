using System.Linq.Expressions;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Infrastructure.Persistence.QuerySpecifications;

public static class SigningKeyQuerySpecifications
{
    public static IQueryable<SigningKey> WhereActive(
        this IQueryable<SigningKey> query,
        DateTimeOffset now)
        => query.Where(k => k.RevokedAt == null && k.ExpiresAt > now);
}