using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Application.SigningKeys.Services;

public interface ISigningKeyService
{
    Task<SigningKeyInfo> CreateAsync(SigningAlgorithm algorithm, TimeSpan? lifetime = null, CancellationToken ct = default);
    Task RevokeAsync(SigningKeyId id, CancellationToken ct = default);
}