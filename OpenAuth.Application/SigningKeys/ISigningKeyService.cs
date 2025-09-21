using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.SigningKeys;

public interface ISigningKeyService
{
    Task<SigningKey> GetLatestAsync(CancellationToken cancellationToken = default);
    Task<SigningKey> GetByIdAsync(SigningKeyId id, CancellationToken cancellationToken = default);
    Task<SigningKey> GetActiveByIdAsync(SigningKeyId id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<SigningKey>> GetAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SigningKey>> GetActiveAsync(CancellationToken cancellationToken = default);
    
    Task<SigningKey> CreateAsync(SigningKey key, CancellationToken cancellationToken = default);
    
    Task<bool> RevokeAsync(SigningKeyId id, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(SigningKeyId id, CancellationToken cancellationToken = default);
}