using System.Text.Json.Serialization;

namespace OpenAuth.Server.Connect.Jwks;

public record JwksResponse([property: JsonPropertyName("keys")] IReadOnlyList<BaseJwkResponse> Keys);