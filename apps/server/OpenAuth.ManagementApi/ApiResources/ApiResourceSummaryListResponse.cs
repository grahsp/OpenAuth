namespace OpenAuth.ManagementApi.ApiResources;

public sealed record ApiResourceSummaryListResponse(IEnumerable<ApiResourceSummaryResponse> Items);