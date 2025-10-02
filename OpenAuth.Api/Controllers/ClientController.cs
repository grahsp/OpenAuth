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
    private readonly IClientService _clientService;
    private readonly ISecretService _secretService;
    
    public ClientController(IClientService clientService, ISecretService secretService)
    {
        _clientService = clientService;
        _secretService = secretService;
    }


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
    public async Task<ActionResult<CreateClientResponse>> Create([FromBody] RegisterClientRequest request)
    {
        var client = await _clientService.RegisterAsync(new ClientName(request.Name));
        var creationResult = await _secretService.AddSecretAsync(client.Id);

        return CreatedAtAction(
            nameof(GetById), 
            new { clientId = client.Id },
            new CreateClientResponse
            {
                ClientId = client.Id.ToString(),
                Name = client.Name.Value,
                CreatedAt = client.CreatedAt,
                SecretId = creationResult.SecretId,
                ClientSecret = creationResult.PlainTextSecret,
                SecretExpiresAt = creationResult.ExpiresAt
            }
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
}