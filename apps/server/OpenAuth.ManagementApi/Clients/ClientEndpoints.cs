using Microsoft.AspNetCore.Mvc;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Commands.CreateClient;
using OpenAuth.Application.Clients.Commands.GrantApiAccess;
using OpenAuth.Application.Clients.Commands.RevokeApiAccess;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Queries.GetClientDetails;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.ApiResources.ValueObjects;
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
		group.MapGet("/{id}", GetClient);

		group.MapPost("/", CreateClient);
		group.MapDelete("/{id}", DeleteClient);

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
	
	private static async Task<IResult> GetClient(
		ClientId id,
		[FromServices] IQueryHandler<GetClientDetailsQuery, ClientDetails?> handler,
		CancellationToken ct)
	{
		var query = new GetClientDetailsQuery(id);
		var client = await handler.HandleAsync(query, ct);

		return client is null
			? Results.NotFound()
			: Results.Ok(client);
	}

	private static async Task<IResult> CreateClient(
		[FromBody] CreateClientRequest request,
		[FromServices] ICommandHandler<CreateClientCommand, CreateClientResult> handler,
		CancellationToken ct)
	{
		var type = ClientApplicationTypes.Parse(request.Type);

		ApiResourceId? apiId = null;
		if (request.ApiId is not null)
		{
			if (!ApiResourceId.TryParse(request.ApiId, out var parsed))
				throw new BadHttpRequestException("Invalid apiId");

			apiId = parsed;
		}

		ScopeCollection? scopes = null;
		if (request.Scopes is not null)
		{
			if (!ScopeCollection.TryParse(request.Scopes, out var parsed))
				throw new BadHttpRequestException("Invalid scopes");

			scopes = parsed;
		}		
		
		var command = new CreateClientCommand(
			type,
			new ClientName(request.Name),
			apiId,
			scopes
		);
		
		var result = await handler.HandleAsync(command, ct);
		return Results.Ok(result.ToResponse());
	}


	private static async Task<IResult> DeleteClient(
		ClientId id,
		IClientService service,
		CancellationToken ct)
	{
		await service.DeleteAsync(id, ct);
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