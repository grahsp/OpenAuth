using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.ApiResources.Queries;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.ApiResources.Queries;

public sealed class GetApiListQueryHandler(AppDbContext context) : IQueryHandler<GetApiListQuery, IReadOnlyList<ApiView>>
{
	public async Task<IReadOnlyList<ApiView>> HandleAsync(GetApiListQuery query, CancellationToken ct)
	{
		return await context.ApiResources
			.Select(x => new ApiView
			(
				x.Id.Value,
				x.ResourceName.Value,
				x.Audience.Value,
				x.Permissions.Select(p => new PermissionView
				(
					p.Scope.Value,
					p.Description != null
						? p.Description.Value
						: null
				)).ToList()
			))
			.ToListAsync(ct);
	}
}