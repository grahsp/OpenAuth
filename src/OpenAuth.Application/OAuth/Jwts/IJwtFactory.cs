using OpenAuth.Domain.OAuth;

namespace OpenAuth.Application.OAuth.Jwts;

public interface IJwtFactory
{
    Task<AccessTokenResult> Create(JwtDescriptor descriptor, CancellationToken ct = default);
}