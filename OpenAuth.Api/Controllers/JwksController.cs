using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Api.Mappers;
using OpenAuth.Application.SigningKeys;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route("api/.well-known")]
public class JwksController : ControllerBase
{
    public JwksController(ISigningKeyService service)
    {
        _service = service;
    }
    
    private readonly ISigningKeyService _service;
    
    
    [HttpGet("jwks.json")]
    public async Task<IActionResult> Get()
    {
        var keys = await _service.GetActiveAsync();
        var response = new JwksResponse(keys.Select(SigningKeyMapper.ToJwk));
        
        return Ok(response);
    }
}