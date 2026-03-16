using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.SigningKeys.Services;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.ManagementApi.SigningKeys;

public static class SigningKeyEndpoints
{
	const string BaseUrl = "/api/keys";
    
	public static IEndpointRouteBuilder MapSigningKeyEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup(BaseUrl);

		group.MapGet("/", GetAll);
		group.MapPost("/", Create);
		group.MapGet("/{signingKeyId}", Get);
		group.MapDelete("/{keyId:guid}", Revoke);

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
		return Results.Created($"{BaseUrl}/{key.Id}", SigningKeyMapper.ToResponse(key));
	}

	private static async Task<IResult> Get(string signingKeyId, ISigningKeyQueryService queryService)
	{
		var signingKey = await queryService.GetByIdAsync(SigningKeyId.Create(signingKeyId));

		if (signingKey is null)
			return Results.NotFound();

		return Results.Ok(SigningKeyMapper.ToResponse(signingKey));
	}

	private static async Task<IResult> Revoke(Guid keyId, ISigningKeyService commandService)
	{
		await commandService.RevokeAsync(new SigningKeyId(keyId));
		return Results.NoContent();
	}
}