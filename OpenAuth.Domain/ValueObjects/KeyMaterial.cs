using OpenAuth.Domain.Enums;

namespace OpenAuth.Domain.ValueObjects;

public record KeyMaterial(Key Key, SigningAlgorithm Alg, KeyType Kty);