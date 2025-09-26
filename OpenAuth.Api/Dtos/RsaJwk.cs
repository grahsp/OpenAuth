namespace OpenAuth.Api.Dtos;

public record RsaJwk(string Kid, string Alg, string Use, string N, string E) : Jwk(Kid, JwkKty.RSA, Alg, Use);