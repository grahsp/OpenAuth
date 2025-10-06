using OpenAuth.Application.Dtos;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Queries;

public interface ISigningKeyQueryService
{
    Task<SigningKeyData?> GetCurrentKeyDataAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<SigningKeyData>> GetActiveKeyDataAsync(CancellationToken ct = default);
    Task<SigningKeyInfo?> GetByIdAsync(SigningKeyId id, CancellationToken ct = default);
    Task<IReadOnlyCollection<SigningKeyInfo>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<SigningKeyInfo>> GetActiveAsync(CancellationToken ct = default);
}