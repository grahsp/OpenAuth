using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.ManagementApi.SigningKeys;

public record SigningKeyRequest(SigningAlgorithm Algorithm, TimeSpan? Lifetime);