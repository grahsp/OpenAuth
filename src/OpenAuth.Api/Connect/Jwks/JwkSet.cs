using System.Text.Json.Serialization;

namespace OpenAuth.Api.Identity.Jwks;

public record JwkSet([property: JsonPropertyName("keys")] IReadOnlyList<BaseJwk> Keys);