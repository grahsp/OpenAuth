using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.SigningKeys;

public interface ISigningKeyRepository
{
    Task<SigningKey?> GetByIdAsync(SigningKeyId id, CancellationToken cancellationToken = default);
    Task<SigningKey?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SigningKey>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SigningKey>> GetActiveAsync(CancellationToken cancellationToken = default);
    
    void Add(SigningKey key);
    void Remove(SigningKey key);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}