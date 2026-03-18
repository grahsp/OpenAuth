namespace OpenAuth.Application.ApiResources.Queries.GetApiSummaryList;

public sealed record ApiSummaryView(
	string Id,
	string Name,
	string Audience
);