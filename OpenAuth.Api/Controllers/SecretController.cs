using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Application.Clients;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route("clients/{clientId:guid}/secrets/")]
public class SecretController : ControllerBase
{
    private readonly ISecretService _secretService;
    
    public SecretController(ISecretService secretService)
    {
        _secretService = secretService;
    }

    
    [HttpGet("{secretId:guid}")]
    public async Task<ActionResult<SecretSummaryResponse>> GetClientSecret(Guid secretId)
    {
        return Ok();
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SecretSummaryResponse>>> GetClientSecrets(Guid clientId)
    {
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult<RegisterClientSecretResponse>> AddClientSecret(Guid clientId)
    {
        var creationResult = await _secretService.AddSecretAsync(new ClientId(clientId));

        return CreatedAtAction(
            nameof(GetClientSecret), 
            new { secretId = creationResult.SecretId },
            creationResult
        );
    }
    
    [HttpDelete("{secretId:guid}")]
    public async Task<ActionResult> DeleteClientSecret(Guid clientId, Guid secretId)
    {
        await _secretService.RevokeSecretAsync(new ClientId(clientId), new SecretId(secretId));
        return NoContent();
    }
}