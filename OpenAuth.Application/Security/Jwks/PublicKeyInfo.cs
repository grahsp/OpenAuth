using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Security.Jwks;

public abstract record PublicKeyInfo
(
    SigningKeyId Kid,
    SigningAlgorithm Alg,
    string Use = "sig"
);