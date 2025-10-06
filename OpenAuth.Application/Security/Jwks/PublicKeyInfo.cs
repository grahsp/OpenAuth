using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Application.Security.Jwks;

public abstract record PublicKeyInfo
(
    SigningKeyId Kid,
    SigningAlgorithm Alg,
    string Use = "sig"
);