using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Application.Clients;
using OpenAuth.Application.Queries;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route("api/clients/{clientId:guid}/audiences")]
public class AudienceController : ControllerBase
{
    private readonly IAudienceService _commandService;
    private readonly IClientQueryService _queryService;
    
    public AudienceController(IAudienceService commandService, IClientQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }
    
    
    // ==================== Audience Operations ==================== //
    
    [HttpGet]
    public async Task<ActionResult<AudienceResponse>> GetAudience(Guid clientId, string name)
    {
        var client = await _queryService.GetDetailsAsync(new ClientId(clientId));
        if (client is null)
            return NotFound();

        var audience =
            client.Audiences
                .SingleOrDefault(a =>
                    a.Name.Value.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (audience is null)
            return NotFound();

        return Ok(audience);
    }
    
    [HttpPost]
    public async Task<ActionResult<AudienceResponse>> AddAudience(
        Guid clientId, 
        [FromBody] RegisterAudienceRequest request)
    {
        var audienceName = new AudienceName(request.Name);
        var response = await _commandService.AddAudienceAsync(new ClientId(clientId), audienceName);

        return CreatedAtAction(
            nameof(GetAudience),
            new { clientId, audienceName },
            response
        );
    }
    
    [HttpDelete("{name}")]
    public async Task<ActionResult> DeleteAudience(Guid clientId, string name)
    {
        await _commandService.RemoveAudienceAsync(
            new ClientId(clientId), 
            new AudienceName(name));
            
        return NoContent();
    }
    
    
    // ==================== Scope Operations ==================== //
    
    [HttpPut("{name}/scopes")]
    public async Task<ActionResult<AudienceResponse>> SetScopes(
        Guid clientId, 
        string name,
        [FromBody] ScopesRequest request)
    {
        var scopes = request.Scopes.Select(x => new Scope(x));
        var response = await _commandService.SetScopesAsync(
            new ClientId(clientId), 
            new AudienceName(name), 
            scopes);
        
        return Ok(response);
    }
    
    [HttpPost("{name}/scopes")]
    public async Task<ActionResult<AudienceResponse>> GrantScopes(
        Guid clientId, 
        string name,
        [FromBody] ScopesRequest request)
    {
        var scopes = request.Scopes.Select(x => new Scope(x));
        var response = await _commandService.GrantScopesAsync(
            new ClientId(clientId), 
            new AudienceName(name), 
            scopes);
        
        return Ok(response);
    }
    
    [HttpDelete("{name}/scopes")]
    public async Task<ActionResult> RevokeScopes(
        Guid clientId, 
        string name,
        [FromBody] ScopesRequest request)
    {
        var scopes = request.Scopes.Select(x => new Scope(x));
        var response = await _commandService.RevokeScopesAsync(
            new ClientId(clientId),
            new AudienceName(name), 
            scopes);
        
        return Ok(response);
    }
}