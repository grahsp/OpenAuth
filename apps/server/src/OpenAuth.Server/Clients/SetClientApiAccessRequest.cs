namespace OpenAuth.Server.Clients;

public sealed record SetClientApiAccessRequest(
	IEnumerable<string> Scopes
);