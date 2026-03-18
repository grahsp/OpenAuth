using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.ApiResources.Queries.GetApiDetails;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.ApiResources.Queries.GetApiDetails;

public sealed class GetApiDetailsQueryHandler(AppDbContext context) : IQueryHandler<GetApiDetailsQuery, ApiDetailsView>
{
	public async Task<ApiDetailsView> HandleAsync(GetApiDetailsQuery query, CancellationToken ct)
	{
		var result = await context.ApiResources
			.Where(x => x.Id == query.Id)
			.Select(x => new
			{
				Id = x.Id.Value,
				Name = x.ResourceName.Value,
				Audience = x.Audience.Value,
				Permissions = x.Permissions.Select(p => new
				{
					Scope = p.Scope.Value,
					Description = p.Description != null
						? p.Description.Value
						: null
				}).ToList()
			})
			.SingleAsync(ct);

		return new ApiDetailsView(
			result.Id.ToString(),
			result.Name,
			result.Audience,
			result.Permissions
				.Select(x => new PermissionView(
					x.Scope,
					x.Description
				))
				.ToList()
		);
	}
}