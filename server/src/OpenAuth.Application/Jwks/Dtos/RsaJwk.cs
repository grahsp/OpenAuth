using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Application.Jwks.Dtos;

public sealed record RsaJwk
(
    SigningKeyId Kid,
    SigningAlgorithm Alg,
    string N,
    string E
) : BaseJwk(Kid, Alg);