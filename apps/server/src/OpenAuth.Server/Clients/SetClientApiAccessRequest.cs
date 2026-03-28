namespace OpenAuth.AuthorizationApi.Clients;

public sealed record SetClientApiAccessRequest(
	IEnumerable<string> Scopes
);