using System.Text.Json.Serialization;

namespace OpenAuth.Api.Dtos;

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Jwk.Kty))]
[JsonDerivedType(typeof(RsaJwk), typeDiscriminator: JwkKty.RSA)]
public abstract record Jwk(string Kid, string Kty, string Alg, string Use);