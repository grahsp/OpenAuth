using OpenAuth.Application.Jwks.Dtos;

namespace OpenAuth.Application.Jwks.Services;

public interface IJwksService
{
    Task<IEnumerable<PublicKeyInfo>> GetJwksAsync();
}