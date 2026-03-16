using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.ManagementApi.Clients;

public static class ClientEndpoints
{
    private const string BaseRoute = "api/clients";
    
    public static IEndpointRouteBuilder MapClientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BaseRoute);

        group.MapGet("/", GetClients);
        group.MapPost("/", CreateClient);
        group.MapGet("/{clientId}", GetClient);
        group.MapDelete("/{clientId}", DeleteClient);

        group.MapPost("/{clientId}/apis/{apiResourceId}", GrantApiAccess);
        group.MapDelete("/{clientId}/apis/{apiResourceId}", RevokeApiAccess);
        
        return app;
    }
    
    private static async Task<IResult> GetClients(
        IClientQueryService service,
        CancellationToken ct)
    {
        var clients = await service.GetPagedAsync(1, 20, ct);
        return Results.Ok(clients.Items);
    }
    
    private static async Task<IResult> CreateClient(
        ClientCreationRequest dto,
        IClientService service,
        CancellationToken ct)
    {
        var request = new CreateClientRequest(
            ClientApplicationTypes.Parse(dto.Type),
            new ClientName(dto.Name),
            dto.RedirectUris.Select(RedirectUri.Parse)
        );
            
        var result = await service.RegisterAsync(request, ct);
        return Results.Ok(result.ToResponse());
    }
    
    private static async Task<IResult> GetClient(
        ClientId clientId,
        IClientQueryService service,
        CancellationToken ct)
    {
        var client = await service.GetByIdAsync(clientId, ct);
        return Results.Ok(client);
    }

    private static async Task<IResult> DeleteClient(
        ClientId clientId,
        IClientService service,
        CancellationToken ct)
    {
        await service.DeleteAsync(clientId, ct);
        return Results.NotFound();
    }

    private static async Task<IResult> GrantApiAccess(
        ClientId clientId,
        ApiResourceId apiResourceId,
        string scope,
        IClientService service,
        CancellationToken ct)
    {
        var scopes = ScopeCollection.Parse(scope);
        var client = await service.GrantApiAccessAsync(clientId, apiResourceId, scopes, ct);
        
        return Results.Ok(client);
    }

    private static async Task<IResult> RevokeApiAccess(
        ClientId clientId,
        ApiResourceId apiResourceId,
        IClientService service,
        CancellationToken ct)
    {
        var client = await service.RevokeApiAccessAsync(clientId, apiResourceId, ct);
        return Results.Ok(client);      
    }
}