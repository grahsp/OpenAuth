namespace OpenAuth.Application.Clients.Queries.GetClientApiAccess;

public sealed record ClientPermissions(
	Guid Id,
	IReadOnlyList<string> Scopes
);