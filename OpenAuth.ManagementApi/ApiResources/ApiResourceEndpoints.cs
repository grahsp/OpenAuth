using OpenAuth.Application.Abstractions;
using OpenAuth.Application.ApiResources.Commands.AddApiResourcePermissions;
using OpenAuth.Application.ApiResources.Commands.CreateApiResource;
using OpenAuth.Application.ApiResources.Commands.DeleteApiResource;
using OpenAuth.Domain.ApiResources.ValueObjects;

namespace OpenAuth.ManagementApi.ApiResources;

public static class ApiResourceEndpoints
{
	private const string BaseRoute = "/api/api-resources";
	
	public static IEndpointRouteBuilder MapApiResourceEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup(BaseRoute);
		
		group.MapPost("/", CreateApiResource);
		group.MapDelete("/{apiResourceId}", DeleteApiResource);
		
		group.MapPost("/{apiResourceId}/permissions", AddApiResourcePermissions);

		return app;
	}

	public static async Task<IResult> CreateApiResource(
		CreateApiResourceRequest request,
		ICommandHandler<CreateApiResourceCommand, CreateApiResourceResult> handler,
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

	public static async Task<IResult> DeleteApiResource(
		ApiResourceId apiResourceId,
		ICommandHandler<DeleteApiResourceCommand> handler,
		CancellationToken ct)
	{
		await handler.HandleAsync(new DeleteApiResourceCommand(apiResourceId), ct);
		return Results.NoContent();
	}

	public static async Task<IResult> AddApiResourcePermissions(
		ApiResourceId apiResourceId,
		AddApiResourcePermissionRequest request,
		ICommandHandler<AddApiResourcePermissionCommand> handler,
		CancellationToken ct)
	{
		var permissions = request.Permissions
			.Select(p => Permission.Parse(p.Key, p.Value));
		
		var command = new AddApiResourcePermissionCommand(apiResourceId, permissions);
		await handler.HandleAsync(command, ct);
		
		return Results.Ok();
	}
}