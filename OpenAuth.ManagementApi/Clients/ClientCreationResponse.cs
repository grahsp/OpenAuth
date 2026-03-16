using System.Text.Json.Serialization;

namespace OpenAuth.ManagementApi.Clients;

public sealed record ClientCreationResponse(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("secret")] string? Secret
);