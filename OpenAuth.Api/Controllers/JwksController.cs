using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Api.Mappers;
using OpenAuth.Application.Security.Jwks;
using OpenAuth.Application.SigningKeys;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route(".well-known")]
public class JwksController : ControllerBase
{
    public JwksController(ISigningKeyService service, IPublicKeyInfoFactory publicKeyInfoFactory)
    {
        _service = service;
        _publicKeyInfoFactory = publicKeyInfoFactory;
    }
    
    private readonly ISigningKeyService _service;
    private readonly IPublicKeyInfoFactory _publicKeyInfoFactory;
    
    
    [HttpGet("jwks.json")]
    public async Task<IActionResult> Get()
    {
        var keys = await _service.GetActiveAsync();
        var response = new JwksResponse(
            keys.Select(k =>
                {
                    var publicKeyInfo = _publicKeyInfoFactory.Create(k.Id, k.KeyMaterial);
                    return publicKeyInfo.ToJwk();
                }
            ));
        
        return Ok(response);
    }
}