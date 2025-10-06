using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Api.Mappers;
using OpenAuth.Application.Clients;
using OpenAuth.Application.Queries;
using OpenAuth.Domain.ValueObjects;

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
     public async Task<ActionResult<CreateClientResponse>> Create([FromBody] RegisterClientRequest request)
     {
         var client = await _clientService.RegisterAsync(new ClientName(request.Name));
         var creationResult = await _secretService.AddSecretAsync(client.Id);
     
         var response = new CreateClientResponse
         {
             ClientId = client.Id.ToString(),
             Name = client.Name.Value,
             CreatedAt = client.CreatedAt,
             SecretId = creationResult.SecretId.ToString(),
             ClientSecret = creationResult.PlainTextSecret,
             SecretExpiresAt = creationResult.ExpiresAt
         };
     
         return CreatedAtAction(nameof(GetById), new { clientId = client.Id }, response);
     }

     [HttpDelete("{clientId:guid}")]
     public async Task<ActionResult> Delete(Guid clientId)
     {
         await _clientService.DeleteAsync(new ClientId(clientId));
         return NoContent();
     }
}