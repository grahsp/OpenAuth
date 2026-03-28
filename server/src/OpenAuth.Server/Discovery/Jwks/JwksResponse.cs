using System.Text.Json.Serialization;

namespace OpenAuth.Server.Discovery.Jwks;

public record JwksResponse([property: JsonPropertyName("keys")] IReadOnlyList<BaseJwkResponse> Keys);