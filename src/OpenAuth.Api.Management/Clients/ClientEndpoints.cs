using OpenAuth.Api.Management.Contracts;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Api.Management.Clients;

public static class ClientEndpoints
{
    public static IEndpointRouteBuilder MapClientEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/management/clients", async (IClientService service, CreateClientResponseDto dto) =>
        {
            var command = new CreateClientCommand(
                ClientApplicationTypes.Parse(dto.Type),
                ClientName.Create(dto.Name),
                [],
                dto.RedirectUris.Select(RedirectUri.Create)
            );
            
            var result = await service.CreateClientAsync(command);
            
            return Results.Created(
                $"/management/clients/{result.Client.Id}",
                result.ToResponse());
        });
        
        app.MapGet("/management/clients", async ([AsParameters] PagedRequestDto request, IClientQueryService service) =>
        {
            if (request.Page is <= 0 or > 100)
                return Results.BadRequest("Page size must be between 1 and 100.");
            
            if (request.PageSize <= 0)
                return Results.BadRequest("Page size must be greater than 0.");

            var result = await service.GetPagedAsync(request.Page, request.PageSize);
            var response = result.ToResponse(c => c.ToResponse());
            
            return Results.Ok(response);
        });

        app.MapGet("/management/clients/{id}", async (string id, IClientQueryService service) =>
        {
            if (!ClientId.TryCreate(id, out var clientId))
                return Results.BadRequest("Malformed client id.");

            var client = await service.GetDetailsAsync(clientId);
            if (client is null)
                return Results.NotFound();
     
            return Results.Ok(client.ToResponse());
        });

        app.MapDelete("/management/clients/{id}", async (string id, IClientService service) =>
        {
            if (!ClientId.TryCreate(id, out var clientId))
                return Results.BadRequest("Malformed client id.");

            await service.DeleteAsync(clientId);
            return Results.NoContent();
        });
        
        return app;
    }
}