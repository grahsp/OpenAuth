using Microsoft.AspNetCore.Mvc;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.ApiResources.Commands.AddApiResourcePermissions;
using OpenAuth.Application.ApiResources.Commands.CreateApiResource;
using OpenAuth.Application.ApiResources.Commands.DeleteApiResource;
using OpenAuth.Application.ApiResources.Commands.RemoveApiResourcePermissions;
using OpenAuth.Application.ApiResources.Queries;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.ManagementApi.ApiResources;

public static class ApiEndpoints
{
	private const string BaseRoute = "/api/apis";
	
	public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup(BaseRoute);
		
		group.MapGet("/", GetApis);
		group.MapGet("/{id}", GetApi);
		
		group.MapPost("/", CreateApi);
		group.MapDelete("/{id}", DeleteApi);
		
		group.MapPost("/{id}/permissions", AddApiPermissions);
		group.MapDelete("/{id}/permissions", RemoveApiPermissions);

		return app;
	}

	public static async Task<IResult> GetApis(
		[FromServices] IQueryHandler<GetApiListQuery, IReadOnlyList<ApiView>> handler,
		CancellationToken ct)
	{
		var query = new GetApiListQuery();
		var apis = await handler.HandleAsync(query, ct);
		
		return Results.Ok(apis);
	}
	
	public static async Task<IResult> GetApi(
		ApiResourceId id,
		[FromServices] IQueryHandler<GetApiQuery, ApiView?> handler,
		CancellationToken ct)
	{
		var query = new GetApiQuery(id);
		var api = await handler.HandleAsync(query, ct);
		
		if (api is null)
			return Results.NotFound();
		
		return Results.Ok(api);
	}

	public static async Task<IResult> CreateApi(
		[FromBody] CreateApiResourceRequest request,
		[FromServices] ICommandHandler<CreateApiResourceCommand, CreateApiResourceResult> handler,
		CancellationToken ct)
	{
		if (!AudienceIdentifier.TryParse(request.AudienceIdentifier, out var audience))
			return Results.BadRequest();

		if (request.Permissions.Count == 0)
			return Results.BadRequest();

		var permissions = request.Permissions
			.Select(p => Permission.Parse(p.Key, p.Value));
		
		var command = new CreateApiResourceCommand(
			new ApiResourceName(request.Name),
			audience,
			permissions);
		
		var result = await handler.HandleAsync(command, ct);
		return Results.Ok(result.Id);
	}

	public static async Task<IResult> DeleteApi(
		[FromRoute] ApiResourceId id,
		[FromServices] ICommandHandler<DeleteApiResourceCommand> handler,
		CancellationToken ct)
	{
		await handler.HandleAsync(new DeleteApiResourceCommand(id), ct);
		return Results.NoContent();
	}

	public static async Task<IResult> AddApiPermissions(
		[FromRoute] ApiResourceId id,
		[FromBody] AddApiResourcePermissionsRequest request,
		[FromServices] ICommandHandler<AddApiResourcePermissionCommand> handler,
		CancellationToken ct)
	{
		var permissions = request.Permissions
			.Select(permission => Permission.Parse(permission.Key, permission.Value));
		
		var command = new AddApiResourcePermissionCommand(id, permissions);
		await handler.HandleAsync(command, ct);
		
		return Results.NoContent();
	}

	public static async Task<IResult> RemoveApiPermissions(
		[FromRoute] ApiResourceId id,
		[FromBody] RemoveApiResourcePermissionsRequest request,
		[FromServices] ICommandHandler<RemoveApiResourcePermissionsCommand> handler,
		CancellationToken ct)
	{
		var scopes = request.Scopes.Select(x => new Scope(x));
	
		var command = new RemoveApiResourcePermissionsCommand(id, scopes);
		await handler.HandleAsync(command, ct);
		
		return Results.NoContent();
	}
}