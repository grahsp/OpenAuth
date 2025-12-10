using OpenAuth.Application.Jwks.Dtos;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Application.SigningKeys.Interfaces;

namespace OpenAuth.Application.Jwks.Services;

public class JwksService : IJwksService
{
    private readonly ISigningKeyQueryService _queryService;
    private readonly IJwkFactory _jwkFactory;
    
    public JwksService(ISigningKeyQueryService queryService, IJwkFactory jwkFactory)
    {
        _queryService = queryService;
        _jwkFactory = jwkFactory;
    }


    public async Task<IEnumerable<BaseJwk>> GetJwksAsync()
    {
        var keyData = await _queryService.GetActiveKeyDataAsync();
        var publicKeyInfo = keyData
            .Select(_jwkFactory.Create);
        
        return publicKeyInfo;
    }
}