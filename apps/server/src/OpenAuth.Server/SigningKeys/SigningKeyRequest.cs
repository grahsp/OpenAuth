using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Server.SigningKeys;

public record SigningKeyRequest(SigningAlgorithm Algorithm, TimeSpan? Lifetime);