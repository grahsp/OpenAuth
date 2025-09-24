using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.SigningKeys;

public class SigningKeyService : ISigningKeyService
{
    public SigningKeyService(ISigningKeyRepository repository, ISigningKeyFactory keyFactory)
    {
        _repository = repository;
        _keyFactory = keyFactory;
    }

    private readonly ISigningKeyRepository _repository;
    private readonly ISigningKeyFactory _keyFactory;


    public async Task<SigningKey?> GetByIdAsync(SigningKeyId id, CancellationToken cancellationToken = default)
        => await _repository.GetByIdAsync(id, cancellationToken);
    
    public async Task<SigningKey?> GetCurrentAsync(CancellationToken cancellationToken = default)
        => await _repository.GetCurrentAsync(cancellationToken);

    public async Task<IEnumerable<SigningKey>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _repository.GetAllAsync(cancellationToken);

    public async Task<IEnumerable<SigningKey>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _repository.GetActiveAsync(cancellationToken);

    public async Task<SigningKey> CreateAsync(SigningAlgorithm algorithm, DateTime? expiresAt = null, CancellationToken cancellationToken = default)
    {
        var key = _keyFactory.Create(algorithm, expiresAt);
        
        _repository.Add(key);
        await _repository.SaveChangesAsync(cancellationToken);

        return key;
    }

    public async Task<bool> RevokeAsync(SigningKeyId id, CancellationToken cancellationToken = default)
    {
        var key = await GetByIdAsync(id, cancellationToken);
        if (key is null)
            return false;
        
        // TODO: Inject TimeProvider
        key.Revoke(DateTime.UtcNow);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RemoveAsync(SigningKeyId id, CancellationToken cancellationToken = default)
    {
        var key = await GetByIdAsync(id, cancellationToken);
        if (key is null)
            return false;
        
        _repository.Remove(key);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}