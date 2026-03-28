namespace OpenAuth.Server.Management.Clients;

public sealed record CreateClientResponse(
	string Id,
	string? Secret
);