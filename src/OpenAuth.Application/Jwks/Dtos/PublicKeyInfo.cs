using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Application.Jwks.Dtos;

public abstract record PublicKeyInfo
(
    SigningKeyId Kid,
    SigningAlgorithm Alg,
    string Use = "sig"
);