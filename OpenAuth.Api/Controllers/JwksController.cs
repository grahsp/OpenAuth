using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Application.Security.Jwks;
using OpenAuth.Application.SigningKeys;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route(".well-known")]
public class JwksController : ControllerBase
{
    public JwksController(ISigningKeyService service, IJwkFactory jwkFactory)
    {
        _service = service;
        _jwkFactory = jwkFactory;
    }
    
    private readonly ISigningKeyService _service;
    private readonly IJwkFactory _jwkFactory;
    
    
    [HttpGet("jwks.json")]
    public async Task<IActionResult> Get()
    {
        var keys = await _service.GetActiveAsync();
        var response = new JwksResponse(
            keys.Select(k =>
                _jwkFactory.Create(k.Id.ToString(), k.KeyMaterial)
            ));
        
        return Ok(response);
    }
}