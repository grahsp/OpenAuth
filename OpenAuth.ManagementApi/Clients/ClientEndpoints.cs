using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.ManagementApi.Clients;

public static class ClientEndpoints
{
    public static IEndpointRouteBuilder MapClientEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/client", async (IClientService service, ClientCreationRequest dto) =>
        {
            var request = new CreateClientRequest(
                ClientApplicationTypes.Parse(dto.Type),
                new ClientName(dto.Name),
                dto.RedirectUris.Select(RedirectUri.Parse)
            );
            
            var result = await service.RegisterAsync(request);
            return Results.Ok(result.ToResponse());
        });
        
        return app;
    }
}