using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.SigningKeys.Services;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Server.Management.SigningKeys;

public static class SigningKeyEndpoints
{
	public static RouteGroupBuilder MapSigningKeyEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/keys");

		group.MapGet("/", GetAll);
		group.MapPost("/", Create);
		group.MapGet("/{signingKeyId}", Get);
		group.MapDelete("/{signingKeyId}", Revoke);

		return group;
	}

	private static async Task<IResult> GetAll(
		ISigningKeyQueryService queryService,
		CancellationToken ct)
	{
		var keys = await queryService.GetAllAsync(ct);
		return Results.Ok(keys.Select(SigningKeyMapper.ToResponse));
	}

	private static async Task<IResult> Create(
		SigningKeyRequest request,
		ISigningKeyService commandService,
		CancellationToken ct)
	{
		var key = await commandService.CreateAsync(request.Algorithm, request.Lifetime, ct);
		return Results.Created($"keys/{key.Id}", (object?)SigningKeyMapper.ToResponse(key));
	}

	private static async Task<IResult> Get(
		SigningKeyId signingKeyId,
		ISigningKeyQueryService queryService,
		CancellationToken ct)
	{
		var signingKey = await queryService.GetByIdAsync(signingKeyId, ct);

		if (signingKey is null)
			return Results.NotFound();

		return Results.Ok((object?)SigningKeyMapper.ToResponse(signingKey));
	}

	private static async Task<IResult> Revoke(
		SigningKeyId signingKeyId,
		ISigningKeyService commandService,
		CancellationToken ct)
	{
		await commandService.RevokeAsync(signingKeyId, ct);
		return Results.NoContent();
	}
}