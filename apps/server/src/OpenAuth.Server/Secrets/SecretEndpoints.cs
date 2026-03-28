using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Secrets.Services;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Server.Secrets;

public static class SecretEndpoints
{
	private const string BaseRoute = "api/clients/{clientId}/secrets";
	
	public static IEndpointRouteBuilder MapSecretEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup(BaseRoute);

		group.MapGet("/", GetSecrets);
		group.MapPost("/", AddSecret);
		group.MapDelete("/{secretId}", RevokeSecret);

		return app;
	}

	private static async Task<IResult> GetSecrets(
		ClientId clientId,
		ISecretQueryService queryService,
		CancellationToken ct)
	{
		var secrets = await queryService.GetActiveSecretsAsync(clientId, ct);
		return Results.Ok(secrets.Select(SecretMapper.ToResponse));
	}

	private static async Task<IResult> AddSecret(
		ClientId clientId,
		ISecretService commandService,
		CancellationToken ct)
	{
		var result = await commandService.AddSecretAsync(clientId, ct);
		return Results.Ok(result.ToResponse());
	}

	private static async Task<IResult> RevokeSecret(
		ClientId clientId,
		SecretId secretId, ISecretService service,
		CancellationToken ct)
	{
		await service.RevokeSecretAsync(clientId, secretId, ct);
		return Results.NoContent();
	}
}