namespace OpenAuth.Application.ApiResources.Queries.GetApiDetails;

public sealed record ApiDetailsView(
	string Id,
	string Name,
	string Audience,
	IReadOnlyCollection<PermissionView> Permissions
);