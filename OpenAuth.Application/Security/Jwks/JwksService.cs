using OpenAuth.Application.Queries;

namespace OpenAuth.Application.Security.Jwks;

public interface IJwksService
{
    Task<IEnumerable<PublicKeyInfo>> GetJwksAsync();
}

public class JwksService : IJwksService
{
    private readonly ISigningKeyQueryService _queryService;
    private readonly IPublicKeyInfoFactory _publicKeyInfoFactory;
    
    public JwksService(ISigningKeyQueryService queryService, IPublicKeyInfoFactory publicKeyInfoFactory)
    {
        _queryService = queryService;
        _publicKeyInfoFactory = publicKeyInfoFactory;
    }


    public async Task<IEnumerable<PublicKeyInfo>> GetJwksAsync()
    {
        var keyData = await _queryService.GetActiveKeyDataAsync();
        var publicKeyInfo = keyData
            .Select(_publicKeyInfoFactory.Create);
        
        return publicKeyInfo;
    }
}