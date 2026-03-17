using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.ApiResources.Queries.GetApiResourceSummaryList;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.ApiResources.Queries.GetApiResourceSummaryList;

public class ApiResourceSummaryListQueryHandler(AppDbContext context) : IQueryHandler<ApiResourceSummaryList>
{
	public async Task<ApiResourceSummaryList> HandleAsync(CancellationToken ct)
	{
		var apiResources = await context.ApiResources
			.Select(x => new ApiResourceSummary(x.Id, x.ResourceName, x.Audience))
			.ToListAsync(ct);
		
		return new ApiResourceSummaryList(apiResources);
	}
}