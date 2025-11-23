using OpenAuth.Domain.OAuth;

namespace OpenAuth.Application.OAuth.Jwts;

public interface IJwtSigner
{
    Task<string> Create(JwtDescriptor descriptor, CancellationToken ct = default);
}