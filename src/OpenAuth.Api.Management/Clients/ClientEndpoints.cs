using OpenAuth.Application.Clients.Dtos;
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
        
        return app;
    }
}