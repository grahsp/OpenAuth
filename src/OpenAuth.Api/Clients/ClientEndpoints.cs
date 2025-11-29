using System.Text.Json.Serialization;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Api.Clients;

public static class ClientEndpoints
{
    public static IEndpointRouteBuilder MapClientEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/client", async (IClientService service, ClientCreationRequest dto) =>
        {
            var request = new CreateClientRequest(
                ClientApplicationTypes.Parse(dto.Type),
                ClientName.Create(dto.Name),
                [],
                dto.RedirectUris.Select(RedirectUri.Create)
            );
            
            var result = await service.RegisterAsync(request);
            return Results.Ok(result.ToResponse());
        });
        
        return app;
    }
}

public sealed record ClientCreationResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("secret")] string? Secret
);

public sealed record ClientCreationRequest(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("redirect_uris")] IEnumerable<string> RedirectUris
);

public static class ClientMapper
{
    public static ClientCreationResponse ToResponse(this RegisteredClientResponse result)
    {
        return new ClientCreationResponse(result.Client.Id.ToString(), result.ClientSecret);
    }
}