using System.Text.Json.Serialization;

namespace OpenAuth.ManagementApi.Clients;

public sealed record GrantApiAccessRequest(
	[property: JsonPropertyName("scope")] string Scope
);