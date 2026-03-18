using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Commands.CreateClient;
using OpenAuth.Application.Clients.Commands.GrantApiAccess;
using OpenAuth.Application.Clients.Commands.RevokeApiAccess;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.ManagementApi.Clients;

public static class ClientEndpoints
{
	private const string BaseRoute = "api/clients";
    
	public static IEndpointRouteBuilder MapClientEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup(BaseRoute);

		group.MapGet("/", GetClients);
		group.MapGet("/{clientId}", GetClient);
		
		group.MapPost("/m2m", CreateM2MClient);
		group.MapPost("/spa", CreateSpaClient);
		group.MapPost("/web", CreateWebClient);
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
	
	private static async Task<IResult> CreateM2MClient(
		CreateM2MClientRequest dto,
		ICommandHandler<CreateM2MClientCommand, CreateClientResult> handler,
		CancellationToken ct)
	{
		var command = new CreateM2MClientCommand(
			new ClientName(dto.Name),
			ApiResourceId.Parse(dto.ApiId),
			ScopeCollection.Parse(dto.Scopes)
		);
            
		var result = await handler.HandleAsync(command, ct);
		return Results.Ok(result.ToResponse());
	}
    
	private static async Task<IResult> CreateSpaClient(
		CreateSpaClientRequest dto,
		ICommandHandler<CreateSpaClientCommand, CreateClientResult> handler,
		CancellationToken ct)
	{
		var command = new CreateSpaClientCommand(
			new ClientName(dto.Name)
		);
            
		var result = await handler.HandleAsync(command, ct);
		return Results.Ok(result.ToResponse());
	}
	
	private static async Task<IResult> CreateWebClient(
		CreateSpaClientRequest dto,
		ICommandHandler<CreateWebClientCommand, CreateClientResult> handler,
		CancellationToken ct)
	{
		var command = new CreateWebClientCommand(
			new ClientName(dto.Name)
		);
            
		var result = await handler.HandleAsync(command, ct);
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
		GrantApiAccessRequest request,
		ICommandHandler<GrantApiAccessCommand> handler,
		CancellationToken ct)
	{
		if (!ScopeCollection.TryParse(request.Scope, out var scopes))
			return Results.BadRequest(new
			{
				error = "invalid_scope",
				message = "The provided scope string is invalid."
			});
        
		var command = new GrantApiAccessCommand(clientId, apiResourceId, scopes);
		await handler.HandleAsync(command, ct);
        
		return Results.NoContent();
	}

	private static async Task<IResult> RevokeApiAccess(
		ClientId clientId,
		ApiResourceId apiResourceId,
		ICommandHandler<RevokeApiAccessCommand> handler,
		CancellationToken ct)
	{
		var command = new RevokeApiAccessCommand(clientId, apiResourceId);
		
		await handler.HandleAsync(command, ct);
		return Results.NotFound();      
	}
}