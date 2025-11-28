using System.Text.Json.Serialization;
using OpenAuth.Api.Dtos;

namespace OpenAuth.Api.Identity.Jwks;

public sealed record RsaJwk(
     string Kid,
     string Alg,
     string Use,
    [property: JsonPropertyName("n")] string N,
    [property: JsonPropertyName("e")] string E
) : BaseJwk(Kid, JwkKty.RSA, Alg, Use);