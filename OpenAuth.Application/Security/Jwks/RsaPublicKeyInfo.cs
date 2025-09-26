using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Security.Jwks;

public sealed record RsaPublicKeyInfo
(
    SigningKeyId Kid,
    SigningAlgorithm Alg,
    string N,
    string E
) : PublicKeyInfo(Kid, Alg);