using OpenAuth.Domain.Enums;

namespace OpenAuth.Api.Dtos;

public record SigningKeyRequest(SigningAlgorithm Algorithm, DateTime? ExpiresAt);