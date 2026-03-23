namespace OpenAuth.ManagementApi.Clients;

public sealed record SetClientApiAccessRequest(
	IEnumerable<string> Scopes
);