using OpenAuth.Application.Dtos;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.SigningKeys;

public interface ISigningKeyService
{
    Task<SigningKeyInfo> CreateAsync(SigningAlgorithm algorithm, TimeSpan? lifetime = null, CancellationToken ct = default);
    Task RevokeAsync(SigningKeyId id, CancellationToken ct = default);
}