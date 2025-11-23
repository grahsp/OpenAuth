using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Application.SigningKeys.Interfaces;

public interface ISigningKeyQueryService
{
    Task<SigningKeyData?> GetCurrentKeyDataAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<SigningKeyData>> GetActiveKeyDataAsync(CancellationToken ct = default);
    Task<SigningKeyInfo?> GetByIdAsync(SigningKeyId id, CancellationToken ct = default);
    Task<IReadOnlyCollection<SigningKeyInfo>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<SigningKeyInfo>> GetActiveAsync(CancellationToken ct = default);
}