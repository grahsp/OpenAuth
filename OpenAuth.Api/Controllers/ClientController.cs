using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Api.Mappers;
using OpenAuth.Application.Clients;
using OpenAuth.Application.Security.Tokens;
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
        var client = await _clientService.RegisterAsync(new ClientName(request.Name));
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
    
    
    // Audiences
    [HttpGet("{clientId:guid}/audiences")]
    public async Task<ActionResult> GetAudiences(Guid clientId)
    {
        var client = await _clientService.GetByIdAsync(new ClientId(clientId));
        if (client is null)
            return NotFound();

        return Ok(client.Audiences);
    }
    
    [HttpPost("{clientId:guid}/audiences")]
    public async Task<ActionResult<Audience>> AddAudience(Guid clientId, [FromBody] RegisterAudienceRequest request)
    {
        var audience = new Audience(request.Name);
        var client = await _clientService.TryAddAudienceAsync(new ClientId(clientId), audience);
        
        if (client is null)
            return NotFound();

        return CreatedAtAction(
            nameof(GetAudience),
            new { clientId = client.Id, name = audience.Value },
            audience
        );
    }

    [HttpGet("{clientId:guid}/audiences/{name}")]
    public async Task<ActionResult<Audience>> GetAudience(Guid clientId, string name)
    {
        var client = await _clientService.GetByIdAsync(new ClientId(clientId));
        if (client is null)
            return NotFound();

        var audience = client.Audiences.SingleOrDefault(x => x.Value == name);
        if (audience is null)
            return NotFound();
        
        return Ok(audience);
    }

    [HttpDelete("{clientId:guid}/audiences/{name}")]
    public async Task<ActionResult> DeleteAudience(Guid clientId, string name)
    {
        var removed = await _clientService.TryRemoveAudienceAsync(new ClientId(clientId), new Audience(name));
        return removed is null ? NotFound() : NoContent();
    }
    
    
    // Scopes
    [HttpPut("{clientId:guid}/audiences/{name}/scopes")]
    public async Task<ActionResult> SetScopes([FromBody] ScopesRequest request, Guid clientId, string name)
    {
        var scopes = request.Scopes.Select(x => new Scope(x));
        var client = await _clientService.SetScopesAsync(new ClientId(clientId), new Audience(name), scopes);
        
        var response = ClientMapper.ToResponse(client.Audiences.Single(x => x.Value == name));
        return Ok(response);
    }
    
    [HttpPost("{clientId:guid}/audiences/{name}/scopes")]
    public async Task<ActionResult> GrantScopes([FromBody] ScopesRequest request, Guid clientId, string name)
    {
        var scopes = request.Scopes.Select(x => new Scope(x));
        var client = await _clientService.GrantScopesAsync(new ClientId(clientId), new Audience(name), scopes);
        
        var response = ClientMapper.ToResponse(client.Audiences.Single(x => x.Value == name));
        return Ok(response);
    }
    
    [HttpDelete("{clientId:guid}/audiences/{name}/scopes")]
    public async Task<ActionResult> RevokeScopes([FromBody] ScopesRequest request, Guid clientId, string name)
    {
        var scopes = request.Scopes.Select(x => new Scope(x));
        await _clientService.RevokeScopesAsync(new ClientId(clientId), new Audience(name), scopes);
        
        return NoContent();
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