namespace OpenAuth.ManagementApi.Clients;

public sealed record CreateM2MClientRequest(
	string Name,
	string ApiId,
	IEnumerable<string> Scopes
);