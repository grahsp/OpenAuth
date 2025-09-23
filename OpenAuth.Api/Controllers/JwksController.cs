using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Api.Mappers;
using OpenAuth.Application.SigningKeys;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route("api/.well-known")]
public class JwksController : ControllerBase
{
    public JwksController(ISigningKeyService service, IKeyParameterExporter keyParameterExporter)
    {
        _service = service;
        _keyParameterExporter = keyParameterExporter;
    }
    
    private readonly ISigningKeyService _service;
    private readonly IKeyParameterExporter _keyParameterExporter;
    
    
    [HttpGet("jwks.json")]
    public async Task<IActionResult> Get()
    {
        var keys = await _service.GetActiveAsync();
        var response = new JwksResponse(
            keys.Select(k =>
                SigningKeyMapper.ToJwk(
                    k,
                    _keyParameterExporter.Export(k.PrivateKey)
                )
            ));
        
        return Ok(response);
    }
}