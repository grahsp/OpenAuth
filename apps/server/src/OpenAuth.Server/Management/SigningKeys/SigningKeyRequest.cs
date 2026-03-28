using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Server.Management.SigningKeys;

public record SigningKeyRequest(SigningAlgorithm Algorithm, TimeSpan? Lifetime);