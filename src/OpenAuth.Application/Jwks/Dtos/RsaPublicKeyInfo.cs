using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Application.Jwks.Dtos;

public sealed record RsaPublicKeyInfo
(
    SigningKeyId Kid,
    SigningAlgorithm Alg,
    string N,
    string E
) : PublicKeyInfo(Kid, Alg);