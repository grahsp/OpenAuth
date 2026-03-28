using System.Text.Json.Serialization;

namespace OpenAuth.Server.Discovery.Jwks;

public sealed record RsaJwkResponse(
     string Kid,
     string Alg,
     string Use,
    [property: JsonPropertyName("n")] string N,
    [property: JsonPropertyName("e")] string E
) : BaseJwkResponse(Kid, JwkKty.RSA, Alg, Use);