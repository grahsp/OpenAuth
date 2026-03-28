namespace OpenAuth.Server.Management.Clients;

public sealed record SetClientApiAccessRequest(
	IEnumerable<string> Scopes
);