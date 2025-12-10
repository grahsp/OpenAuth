namespace OpenAuth.Api.Connect.Jwks;

public static class JwkMapper
{
    public static JwksResponse ToJwkSet(this IEnumerable<Application.Jwks.Dtos.BaseJwk> infos)
    {
        var set = new List<BaseJwkResponse>();
        
        foreach (var info in infos)
        {
            var jwk = info switch
            {
                Application.Jwks.Dtos.RsaJwk rsa => new RsaJwkResponse(info.Kid.ToString(), info.Alg.ToString(), info.Use, rsa.N, rsa.E),
                _ => throw new NotImplementedException()
            };
            
            set.Add(jwk);
        }

        return new JwksResponse(set);
    }
}