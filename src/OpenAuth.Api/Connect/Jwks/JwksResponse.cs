using System.Text.Json.Serialization;

namespace OpenAuth.Api.Connect.Jwks;

public record JwksResponse([property: JsonPropertyName("keys")] IReadOnlyList<BaseJwkResponse> Keys);