namespace OpenAuth.AuthorizationApi.Clients;

public sealed record CreateClientResponse(
	string Id,
	string? Secret
);