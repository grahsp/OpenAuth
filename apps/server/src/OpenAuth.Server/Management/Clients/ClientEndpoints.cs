using Microsoft.AspNetCore.Mvc;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Commands.CreateClient;
using OpenAuth.Application.Clients.Commands.SetClientApiAccess;
using OpenAuth.Application.Clients.Commands.UpdateClient;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Queries.GetClientApiAccess;
using OpenAuth.Application.Clients.Queries.GetClientDetails;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Server.Management.Clients;

public static class ClientEndpoints
{
	public static RouteGroupBuilder MapClientEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/clients");

		group.MapGet("/", GetClients);
		group.MapGet("/{id}", GetClient);

		group.MapPost("/", CreateClient);
		group.MapPut("/{id}/configuration", UpdateClientConfiguration);
		group.MapDelete("/{id}", DeleteClient);

		group.MapGet("/{clientId}/apis/access", GetClientApiAccess);
		group.MapPut("/{clientId}/apis/{apiResourceId}", GrantApiAccess);
        
		return group;
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
	
	private static async Task<IResult> GetClientApiAccess(
		ClientId clientId,
		[FromServices] IQueryHandler<GetClientPermissionsQuery, IReadOnlyList<ClientPermissions>> handler,
		CancellationToken ct)
	{
		var command = new GetClientPermissionsQuery(clientId);
		var permissions = await handler.HandleAsync(command, ct);
		
		return Results.Ok(permissions);
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

	private static async Task<IResult> UpdateClientConfiguration(
		ClientId id,
		[FromBody] UpdateClientConfigurationRequest request,
		[FromServices] ICommandHandler<UpdateClientConfigurationCommand> handler,
		CancellationToken ct)
	{
		var applicationType = ClientApplicationTypes.Parse(request.ApplicationType);
		var tokenLifetime = TimeSpan.FromSeconds(request.TokenLifetimeInSeconds);

		var redirectUris = request.RedirectUris.Select(x =>
		{
			if (!RedirectUri.TryParse(x, out var uri))
				throw new ValidationException($"Invalid redirect URI: '{x}'");

			return uri;
		}).ToList();

		var allowedGrantTypes = request.AllowedGrantTypes.Select(x =>
		{
			if (!GrantType.TryParse(x, out var grant))
				throw new ValidationException($"Invalid grant type: '{x}'");

			return grant;
		});

		var command = new UpdateClientConfigurationCommand(
			id,
			new ClientName(request.Name),
			applicationType,
			redirectUris,
			tokenLifetime,
			allowedGrantTypes
		);

		await handler.HandleAsync(command, ct);
		return Results.NoContent();
	}

	private static async Task<IResult> GrantApiAccess(
		ClientId clientId,
		ApiResourceId apiResourceId,
		SetClientApiAccessRequest request,
		ICommandHandler<SetClientApiAccessCommand> handler,
		CancellationToken ct)
	{
		if (!ScopeCollection.TryParse(request.Scopes, out var scopes))
			return Results.BadRequest(new
			{
				error = "invalid_scope",
				message = "The provided scopes are invalid."
			});
        
		var command = new SetClientApiAccessCommand(clientId, apiResourceId, scopes);
		await handler.HandleAsync(command, ct);
        
		return Results.NoContent();
	}
}