using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.AuthorizationApi.SigningKeys;

public record SigningKeyRequest(SigningAlgorithm Algorithm, TimeSpan? Lifetime);