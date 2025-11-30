using OpenAuth.Application.Jwks.Dtos;

namespace OpenAuth.Api.Connect.Jwks;

public static class JwkMapper
{
    public static JwkSet ToJwkSet(this IEnumerable<PublicKeyInfo> infos)
    {
        var set = new List<BaseJwk>();
        
        foreach (var info in infos)
        {
            var jwk = info switch
            {
                RsaPublicKeyInfo rsa => new RsaJwk(info.Kid.ToString(), info.Alg.ToString(), info.Use, rsa.N, rsa.E),
                _ => throw new NotImplementedException()
            };
            
            set.Add(jwk);
        }

        return new JwkSet(set);
    }
}