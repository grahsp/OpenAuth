using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Queries.GetClientApiAccess;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.Clients.Queries;

public sealed class GetClientPermissionsQueryHandler(AppDbContext context)
	: IQueryHandler<GetClientPermissionsQuery, IReadOnlyList<ClientPermissions>>
{
	public async Task<IReadOnlyList<ClientPermissions>> HandleAsync(GetClientPermissionsQuery query, CancellationToken ct)
	{
		return await context.Clients
			.Where(client => client.Id == query.ClientId)
			.SelectMany(client => client.Apis)
			.Select(api => new ClientPermissions(
				api.ApiResourceId,
				api.AllowedScopes.ToList()))
			.ToListAsync(ct);
	}
}