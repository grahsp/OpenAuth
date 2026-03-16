using System.Text.Json.Serialization;

namespace OpenAuth.AuthorizationApi.Connect.Jwks;

[JsonDerivedType(typeof(RsaJwkResponse))]
public record BaseJwkResponse(
    [property: JsonPropertyName("kid")] string Kid,
    [property: JsonPropertyName("kty")] string Kty,
    [property: JsonPropertyName("alg")] string Alg,
    [property: JsonPropertyName("use")] string Use);
