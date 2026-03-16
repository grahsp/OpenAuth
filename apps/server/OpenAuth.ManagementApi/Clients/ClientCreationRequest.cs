using System.Text.Json.Serialization;

namespace OpenAuth.ManagementApi.Clients;

public sealed record ClientCreationRequest(
	[property: JsonPropertyName("type")] string Type,
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("redirect_uris")] IEnumerable<string> RedirectUris
);