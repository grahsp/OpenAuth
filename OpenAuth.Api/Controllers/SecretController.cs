using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Api.Mappers;
using OpenAuth.Application.Clients;
using OpenAuth.Application.Secrets;
using OpenAuth.Application.Secrets.Dtos;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Secrets.Services;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route("clients/{clientId:guid}/secrets/")]
public class SecretController : ControllerBase
{
    private readonly ISecretService _commandService;
    private readonly ISecretQueryService _queryService;
    
    public SecretController(ISecretService commandService, ISecretQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SecretInfo>>> GetSecret(Guid clientId)
    {
        var secrets =
            await _queryService.GetActiveSecretsAsync(new ClientId(clientId));
        return Ok(secrets.Select(ClientMapper.ToResponse));
    }

    [HttpPost]
    public async Task<ActionResult<CreatedSecretResponse>> AddSecret(Guid clientId)
    {
        var result = await _commandService.AddSecretAsync(new ClientId(clientId));

        var secret = new SecretResponse(
            result.SecretId.Value,
            clientId,
            result.CreatedAt,
            result.ExpiresAt,
            result.ExpiresAt
        );
        
        var response = new CreatedSecretResponse(
            secret,
            result.PlainTextSecret
        );
        
        return Created(string.Empty, response);
    }
    
    [HttpDelete("{secretId:guid}")]
    public async Task<ActionResult> RevokeSecret(Guid clientId, Guid secretId)
    {
        await _commandService.RevokeSecretAsync(new ClientId(clientId), new SecretId(secretId));
        return NoContent();
    }
}