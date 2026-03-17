using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.ApiResources.Queries.GetApiResourceSummaryList;

public sealed record ApiResourceSummary(
	ApiResourceId Id,
	ApiResourceName Name,
	AudienceIdentifier Audience
);