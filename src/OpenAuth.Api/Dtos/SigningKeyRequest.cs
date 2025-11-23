using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Api.Dtos;

public record SigningKeyRequest(SigningAlgorithm Algorithm, TimeSpan? Lifetime);