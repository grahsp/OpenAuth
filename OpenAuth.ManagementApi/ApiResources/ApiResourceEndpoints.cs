using OpenAuth.Application.Abstractions;
using OpenAuth.Application.ApiResources.Commands.CreateApiResource;
using OpenAuth.Domain.Apis.ValueObjects;

namespace OpenAuth.ManagementApi.ApiResources;

public static class ApiResourceEndpoints
{
	private const string BaseRoute = "/api/api-resources";
	
	public static IEndpointRouteBuilder MapApiResourceEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup(BaseRoute);
		
		group.MapPost("/", CreateApiResource);

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
}