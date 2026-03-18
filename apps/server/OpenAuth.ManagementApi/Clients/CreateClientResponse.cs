namespace OpenAuth.ManagementApi.Clients;

public sealed record CreateClientResponse(
	string Id,
	string? Secret
);