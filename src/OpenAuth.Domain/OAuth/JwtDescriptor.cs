using System.Security.Claims;

namespace OpenAuth.Domain.OAuth;

public record JwtDescriptor(
    string Issuer,
    IReadOnlyCollection<Claim> Claims,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset NotBefore)
{
    public TimeSpan Lifetime => ExpiresAt - IssuedAt;
}