using OpenAuth.Application.Dtos;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Infrastructure.Persistence.QuerySpecifications;

public static class SecretQuerySpecifications
{
    public static IQueryable<ClientSecret> WhereActive(
        this IQueryable<ClientSecret> query,
        DateTimeOffset now)
        => query.Where(k => k.RevokedAt == null && k.ExpiresAt > now);
}