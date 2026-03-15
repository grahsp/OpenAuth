using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.ManagementApi.Dtos;

public record SigningKeyRequest(SigningAlgorithm Algorithm, TimeSpan? Lifetime);