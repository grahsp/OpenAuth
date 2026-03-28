namespace OpenAuth.Server.Management.Clients;

public sealed record CreateClientRequest(
	string Type,
	string Name,
	string? ApiId,
	IEnumerable<string>? Scopes
);