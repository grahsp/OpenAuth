using OpenAuth.Application.ApiResources.Queries.GetApiResourceSummaryList;

namespace OpenAuth.ManagementApi.ApiResources;

public static class ApiResourceMapper
{
	public static ApiResourceSummaryResponse ToResponse(this ApiResourceSummary result)
	{
		return new ApiResourceSummaryResponse(
			result.Id.ToString(),
			result.Name.ToString(),
			result.Audience.ToString()
		);
	}
	
	public static ApiResourceSummaryListResponse ToResponse(this ApiResourceSummaryList result)
	{
		var items = result.Items.Select(ToResponse);
		return new ApiResourceSummaryListResponse(items);
	}
}