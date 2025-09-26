using OpenAuth.Api.Dtos;
using OpenAuth.Application.Security.Jwks;

namespace OpenAuth.Api.Mappers;

public static class JwkMapper
{
    public static Jwk ToJwk(this PublicKeyInfo info)
        => info switch
        {
            RsaPublicKeyInfo rsa => new RsaJwk(info.Kid.ToString(), info.Alg.ToString(), info.Use, rsa.N, rsa.E),
            _ => throw new NotImplementedException()
        };
}