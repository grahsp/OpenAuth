using System.Text.Json.Serialization;

namespace OpenAuth.Api.Connect.Jwks;

public record JwkSet([property: JsonPropertyName("keys")] IReadOnlyList<BaseJwk> Keys);