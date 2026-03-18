using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.ApiResources.Queries.GetApiSummaryList;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.ApiResources.Queries.GetApiSummaryList;

public class GetApiSummaryListQueryHandler(AppDbContext context) : IQueryHandler<GetApiSummaryListQuery, IReadOnlyList<ApiSummaryView>>
{
	public async Task<IReadOnlyList<ApiSummaryView>> HandleAsync(GetApiSummaryListQuery query, CancellationToken ct)
	{
		var apis = await context.ApiResources
			.Select(x => new
			{
				Id = x.Id.Value,
				Name = x.ResourceName.Value,
				Audience = x.Audience.Value
			})
			.ToListAsync(ct);
		
		return apis
			.Select(x => new ApiSummaryView(
				x.Id.ToString(),
				x.Name,
				x.Audience))
			.ToList();
	}
}