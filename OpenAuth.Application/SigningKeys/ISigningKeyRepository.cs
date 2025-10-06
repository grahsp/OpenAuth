using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Application.SigningKeys;

public interface ISigningKeyRepository
{
    Task<SigningKey?> GetByIdAsync(SigningKeyId id, CancellationToken cancellationToken = default);

    void Add(SigningKey key);
    void Remove(SigningKey key);
    Task SaveChangesAsync(CancellationToken ct = default);
}