using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Api.Mappers;
using OpenAuth.Application.Clients;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    public ClientController(IClientService clientService)
    {
        _clientService = clientService;
    }

    private readonly IClientService _clientService;

    [HttpGet("{clientId:guid}")]
    public async Task<ActionResult<Client>> GetById(Guid clientId)
    {
        var client = await _clientService.GetByIdAsync(new ClientId(clientId));
        if (client is null)
            return NotFound();

        var response = ClientMapper.ToResponse(client);
        return Ok(response);
    }
    
    [HttpPost]
    public async Task<ActionResult<RegisterClientResponse>> Create([FromBody] RegisterClientRequest request)
    {
        var client = await _clientService.RegisterAsync(request.Name);
        var creationResult = await _clientService.AddSecretAsync(client.Id);

        var response = ClientMapper.ToResponse(client);
        return CreatedAtAction(
            nameof(GetById), 
            new { clientId = client.Id },
            new RegisterClientResponse(response, creationResult.Plain)
        );
    }

    [HttpDelete("{clientId:guid}")]
    public async Task<ActionResult> Delete(Guid clientId)
    {
        var success = await _clientService.DeleteAsync(new ClientId(clientId));
        return success ? NoContent() : NotFound();
    }
    
    
    // Client Secrets
    [HttpGet("{clientId:guid}/secrets")]
    public async Task<ActionResult<IEnumerable<ClientSecretResponse>>> GetClientSecrets(Guid clientId)
    {
        var client = await _clientService.GetByIdAsync(new ClientId(clientId));
        if (client is null)
            return NotFound();

        var response = client.Secrets.Select(ClientMapper.ToResponse);
        return Ok(response);
    }

    [HttpPost("{clientId:guid}/secrets")]
    public async Task<ActionResult<RegisterClientSecretResponse>> AddClientSecret(Guid clientId)
    {
        var creationResult = await _clientService.AddSecretAsync(new ClientId(clientId), DateTime.UtcNow.AddDays(30));
        
        var secret = ClientMapper.ToResponse(creationResult.Secret);
        var response = new RegisterClientSecretResponse(secret, creationResult.Plain);

        return CreatedAtAction(
            nameof(GetClientSecret), 
            new { secretId = secret.Id },
            response
        );
    }

    [HttpGet("secrets/{secretId:guid}")]
    public async Task<ActionResult<ClientSecretResponse>> GetClientSecret(Guid secretId)
    {
        var secret = await _clientService.GetSecretAsync(new SecretId(secretId));
        if (secret is null)
            return NotFound();

        var response = ClientMapper.ToResponse(secret);
        return Ok(response);
    }

    [HttpPost("secrets/{secretId:guid}/revoke")]
    public async Task<ActionResult> RevokeClientSecret(Guid secretId)
    {
        var success = await _clientService.RevokeSecretAsync(new SecretId(secretId));
        return success ? NoContent() : NotFound();
    }
    
    [HttpDelete("secrets/{secretId:guid}")]
    public async Task<ActionResult> DeleteClientSecret(Guid secretId)
    {
        var success = await _clientService.RemoveSecretAsync(new SecretId(secretId));
        return success ? NoContent() : NotFound();
    }
}