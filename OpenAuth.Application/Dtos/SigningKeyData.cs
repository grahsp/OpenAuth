using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Application.Dtos;

public sealed record SigningKeyData(
    SigningKeyId Kid,
    KeyType Kty,
    SigningAlgorithm Alg,
    Key Key
);