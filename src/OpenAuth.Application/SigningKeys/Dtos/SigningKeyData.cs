using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Application.SigningKeys.Dtos;

public sealed record SigningKeyData(
    SigningKeyId Kid,
    KeyType Kty,
    SigningAlgorithm Alg,
    Key Key
);