namespace OpenAuth.Application.ApiResources.Queries;

public sealed record ApiView(
	Guid Id,
	string Name,
	string Audience,
	IReadOnlyCollection<PermissionView> Permissions
);