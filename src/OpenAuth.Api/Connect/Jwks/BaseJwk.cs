using System.Text.Json.Serialization;

namespace OpenAuth.Api.Connect.Jwks;

[JsonDerivedType(typeof(RsaJwk))]
public record BaseJwk(
    [property: JsonPropertyName("kid")] string Kid,
    [property: JsonPropertyName("kty")] string Kty,
    [property: JsonPropertyName("alg")] string Alg,
    [property: JsonPropertyName("use")] string Use);
