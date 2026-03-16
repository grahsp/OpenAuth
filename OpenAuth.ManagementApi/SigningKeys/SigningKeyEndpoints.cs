using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.SigningKeys.Services;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.ManagementApi.SigningKeys;

public static class SigningKeyEndpoints
{
	private const string BaseRoute = "/api/keys";
    
	public static IEndpointRouteBuilder MapSigningKeyEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup(BaseRoute);

		group.MapGet("/", GetAll);
		group.MapPost("/", Create);
		group.MapGet("/{signingKeyId}", Get);
		group.MapDelete("/{signingKeyId}", Revoke);

		return app;
	}

	private static async Task<IResult> GetAll(ISigningKeyQueryService queryService)
	{
		var keys = await queryService.GetAllAsync();
		return Results.Ok(keys.Select(SigningKeyMapper.ToResponse));
	}

	private static async Task<IResult> Create(SigningKeyRequest request, ISigningKeyService commandService)
	{
		var key = await commandService.CreateAsync(request.Algorithm, request.Lifetime);
		return Results.Created($"{BaseRoute}/{key.Id}", SigningKeyMapper.ToResponse(key));
	}

	private static async Task<IResult> Get(SigningKeyId signingKeyId, ISigningKeyQueryService queryService)
	{
		var signingKey = await queryService.GetByIdAsync(signingKeyId);

		if (signingKey is null)
			return Results.NotFound();

		return Results.Ok(SigningKeyMapper.ToResponse(signingKey));
	}

	private static async Task<IResult> Revoke(SigningKeyId signingKeyId, ISigningKeyService commandService)
	{
		await commandService.RevokeAsync(signingKeyId);
		return Results.NoContent();
	}
}