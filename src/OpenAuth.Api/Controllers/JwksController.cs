using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Mappers;
using OpenAuth.Application.Jwks;
using OpenAuth.Application.Jwks.Services;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route(".well-known")]
public class JwksController : ControllerBase
{
    private readonly IJwksService _service;
    
    public JwksController(IJwksService service)
    {
        _service = service;
    }
    
    
    [HttpGet("jwks.json")]
    public async Task<IActionResult> Get()
    {
        var publicKeyInfo = await _service.GetJwksAsync();
        var response = publicKeyInfo.Select(k => k.ToJwk());
        
        return Ok(response);
    }
}