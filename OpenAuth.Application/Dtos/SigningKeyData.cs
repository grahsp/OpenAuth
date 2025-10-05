using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Dtos;

public sealed record SigningKeyData(
    SigningKeyId Kid,
    KeyType Kty,
    SigningAlgorithm Alg,
    Key Key
);