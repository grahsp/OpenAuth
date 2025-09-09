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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Client>> GetById(Guid id)
    {
        var client = await _clientService.GetByIdAsync(new ClientId(id));
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
            new { id = client.Id },
            new RegisterClientResponse(response, creationResult.Plain)
        );
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _clientService.DeleteAsync(new ClientId(id));
        return success ? NoContent() : NotFound();
    }
}