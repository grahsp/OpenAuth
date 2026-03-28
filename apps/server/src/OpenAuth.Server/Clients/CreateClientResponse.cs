namespace OpenAuth.Server.Clients;

public sealed record CreateClientResponse(
	string Id,
	string? Secret
);