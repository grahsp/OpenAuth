using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.SigningKeys;

public interface ISigningKeyService
{
    Task<SigningKey?> GetByIdAsync(SigningKeyId id, CancellationToken cancellationToken = default);
    Task<SigningKey?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SigningKey>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SigningKey>> GetActiveAsync(CancellationToken cancellationToken = default);
    
    Task<SigningKey> CreateAsync(SigningAlgorithm algorithm, TimeSpan? lifetime = null, CancellationToken cancellationToken = default);
    
    Task<bool> RevokeAsync(SigningKeyId id, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(SigningKeyId id, CancellationToken cancellationToken = default);
}