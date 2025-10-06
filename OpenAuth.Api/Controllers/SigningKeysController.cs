using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Api.Mappers;
using OpenAuth.Application.Queries;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route("api/keys")]
public class SigningKeysController : ControllerBase
{
    private readonly ISigningKeyService _commandService;
    private readonly ISigningKeyQueryService _queryService;
    
    public SigningKeysController(ISigningKeyService commandService, ISigningKeyQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<SigningKeyResponse>>> GetAll()
    {
        var keys = await _queryService.GetAllAsync();
        return Ok(keys.Select(SigningKeyMapper.ToResponse));
    }
    
    [HttpPost]
    public async Task<ActionResult<SigningKeyResponse>> Create([FromBody] SigningKeyRequest request)
    {
        var key = await _commandService.CreateAsync(request.Algorithm, request.Lifetime);
        return CreatedAtAction(nameof(Get), new { keyId = key.Id }, SigningKeyMapper.ToResponse(key));
    }
    
    [HttpGet("{keyId:guid}")]
    public async Task<ActionResult<SigningKeyResponse>> Get(Guid keyId)
    {
        var key = await _queryService.GetByIdAsync(new SigningKeyId(keyId));
        if (key is null)
            return NotFound();
    
        return Ok(SigningKeyMapper.ToResponse(key));
    }

    [HttpDelete("{keyId:guid}")]
    public async Task<ActionResult> Revoke(Guid keyId)
    {
        await _commandService.RevokeAsync(new SigningKeyId(keyId));
        return NoContent();
    }
}