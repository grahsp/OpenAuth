using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Api.Mappers;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Application.Secrets.Services;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly ISecretService _secretService;
    private readonly IClientQueryService _queryService;
    
    public ClientController(IClientService clientService, ISecretService secretService, IClientQueryService queryService)
    {
        _clientService = clientService;
        _secretService = secretService;
        _queryService = queryService;
    }


     [HttpGet("{clientId:guid}")]
     public async Task<ActionResult<ClientResponse>> GetById(Guid clientId)
     {
         var details = await _queryService.GetDetailsAsync(new ClientId(clientId));
         if (details is null)
             return NotFound();
     
         return Ok(ClientMapper.ToResponse(details));
     }
     
     [HttpPost]
     public async Task<ActionResult<CreateClientResponse>> Create([FromBody] RegisterClientCommand command)
     {
         var response = await _clientService.RegisterAsync(command);
         return CreatedAtAction(nameof(GetById), new { clientId = response.Client.Id }, response);
     }

     [HttpDelete("{clientId:guid}")]
     public async Task<ActionResult> Delete(Guid clientId)
     {
         await _clientService.DeleteAsync(new ClientId(clientId));
         return NoContent();
     }
     
     
     // Audiences
     [HttpGet("{clientId}/audiences")]
     public async Task<IActionResult> GetAudiences(string clientId, CancellationToken ct)
     {
         if (!ClientId.TryCreate(clientId, out var id))
             return BadRequest();

         var details = await _queryService.GetDetailsAsync(id, ct);
         return Ok(details?.Audiences
             .Select(a => new
             {
                 Audience = a.Name.NormalizedValue,
                 Scopes = a.Scopes.ToString()
             }) ?? []);
     }
    
     [HttpPost("{clientId}/audiences")]
     public async Task<IActionResult> AddAudience(string clientId, [FromBody] AudienceDto request, CancellationToken ct)
     {
         if (!ClientId.TryCreate(clientId, out var id))
             return BadRequest();
        
         if (!AudienceName.TryCreate(request.Name, out var name))
             return BadRequest();
        
         var audience = new Audience(name, ScopeCollection.Parse(request.Scopes));
         var details = await _clientService.AddAudienceAsync(id, audience, ct);

         return Ok(details);
     }

     [HttpPut("{clientId}/audiences")]
     public async Task<IActionResult> SetAudiences(string clientId, [FromBody] IEnumerable<AudienceDto> request,
         CancellationToken ct)
     {
         if (!ClientId.TryCreate(clientId, out var id))
             return BadRequest();

         var audiences = new List<Audience>();
         foreach (var dto in request)
         {
             if (!AudienceName.TryCreate(dto.Name, out var name))
                 return BadRequest();
             
             var audience = new Audience(name, ScopeCollection.Parse(dto.Scopes));
             audiences.Add(audience);
         }
         
         var details = await _clientService.SetAudiencesAsync(id, audiences, ct);
         return Ok(details);
     }
    
     [HttpDelete("{clientId}/audiences/{audience}")]
     public async Task<IActionResult> RemoveAudience(string clientId, string audience, CancellationToken ct)
     {
         if (!ClientId.TryCreate(clientId, out var id))
             return BadRequest();

         if (!AudienceName.TryCreate(audience, out var name))
             return BadRequest();

         await _clientService.RemoveAudienceAsync(id, name, ct);
         return NoContent();
     }
}