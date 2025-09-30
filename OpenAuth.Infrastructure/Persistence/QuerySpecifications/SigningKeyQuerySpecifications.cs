using System.Linq.Expressions;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Infrastructure.Persistence.QuerySpecifications;

public static class SigningKeyQuerySpecifications
{
    public static Expression<Func<SigningKey, bool>> IsActive(DateTimeOffset now)
        => k => k.RevokedAt == null && k.ExpiresAt > now;
}