using System.Text.Json.Serialization;

namespace OpenAuth.AuthorizationApi.Connect.Jwks;

public record JwksResponse([property: JsonPropertyName("keys")] IReadOnlyList<BaseJwkResponse> Keys);