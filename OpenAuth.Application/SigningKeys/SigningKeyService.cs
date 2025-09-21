using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.SigningKeys;

public class SigningKeyService : ISigningKeyService
{
    public SigningKeyService(ISigningKeyRepository repository)
    {
        _repository = repository;
    }

    private readonly ISigningKeyRepository _repository;


    public async Task<SigningKey?> GetByIdAsync(SigningKeyId id, CancellationToken cancellationToken = default)
        => await _repository.GetByIdAsync(id, cancellationToken);

    public async Task<IEnumerable<SigningKey>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _repository.GetAllAsync(cancellationToken);

    public async Task<IEnumerable<SigningKey>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _repository.GetActiveAsync(cancellationToken);

    public Task<SigningKey> CreateAsync(SigningKey key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RevokeAsync(SigningKeyId id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveAsync(SigningKeyId id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}