using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Api.Mappers;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route("api/keys")]
public class SigningKeysController : ControllerBase
{
    public SigningKeysController(ISigningKeyService service)
    {
        _service = service;
    }
    
    private readonly ISigningKeyService _service;


    [HttpGet]
    public async Task<ActionResult<IEnumerable<SigningKeyResponse>>> GetAll()
    {
        var keys = await _service.GetAllAsync();
        
        var response = keys.Select(SigningKeyMapper.ToResponse).ToList();
        return Ok(response);
    }
    
    [HttpGet("current")]
    public async Task<ActionResult<SigningKeyResponse>> GetCurrent()
    {
        var key = await _service.GetCurrentAsync();
        if (key is null)
            return NotFound();
        
        var response = SigningKeyMapper.ToResponse(key);
        return Ok(response);
    }

    [HttpGet("{keyId:guid}")]
    public async Task<ActionResult<SigningKeyResponse>> Get(Guid keyId)
    {
        var key = await _service.GetByIdAsync(new SigningKeyId(keyId));
        if (key is null)
            return NotFound();

        var response = SigningKeyMapper.ToResponse(key);
        return Ok(response);
    }


    [HttpPost]
    public async Task<ActionResult<SigningKeyResponse>> Create([FromBody] SigningKeyRequest request)
    {
        var key = await _service.CreateAsync(request.Algorithm, request.Lifetime);
        var response = SigningKeyMapper.ToResponse(key);
        
        return CreatedAtAction(nameof(Get), new { keyId = key.Id }, response);
    }


    [HttpPost("{keyId:guid}/revoke")]
    public async Task<ActionResult> Revoke(Guid keyId)
    {
        var result = await _service.RevokeAsync(new SigningKeyId(keyId));
        if (!result)
            return BadRequest("Could not revoke key.");

        return NoContent();
    }


    [HttpDelete("{keyId:guid}")]
    public async Task<ActionResult> Remove(Guid keyId)
    {
        var result = await _service.RemoveAsync(new SigningKeyId(keyId));
        if (!result)
            return BadRequest("Could not remove key.");
        
        return NoContent();
    }
}