using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.SigningKeys;

public interface ISigningKeyService
{
    Task<SigningKey> CreateAsync(SigningAlgorithm algorithm, TimeSpan? lifetime = null, CancellationToken ct = default);
    Task RevokeAsync(SigningKeyId id, CancellationToken ct = default);
}