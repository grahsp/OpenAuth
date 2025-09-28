using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.SigningKeys;

public class SigningKeyService : ISigningKeyService
{
    public SigningKeyService(ISigningKeyRepository repository, ISigningKeyFactory keyFactory, TimeProvider time)
    {
        _repository = repository;
        _keyFactory = keyFactory;
        _time = time;
    }

    private readonly ISigningKeyRepository _repository;
    private readonly ISigningKeyFactory _keyFactory;
    private readonly TimeProvider _time;


    public async Task<SigningKey?> GetByIdAsync(SigningKeyId id, CancellationToken cancellationToken = default)
        => await _repository.GetByIdAsync(id, cancellationToken);
    
    public async Task<SigningKey?> GetCurrentAsync(CancellationToken cancellationToken = default)
        => await _repository.GetCurrentAsync(_time.GetUtcNow().DateTime, cancellationToken);

    public async Task<IEnumerable<SigningKey>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _repository.GetAllAsync(cancellationToken);

    public async Task<IEnumerable<SigningKey>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _repository.GetActiveAsync(_time.GetUtcNow().DateTime, cancellationToken);

    public async Task<SigningKey> CreateAsync(SigningAlgorithm algorithm, TimeSpan? lifetime = null, CancellationToken cancellationToken = default)
    {
        var now = _time.GetUtcNow().DateTime;
        var key = _keyFactory.Create(algorithm, now, lifetime);
        
        _repository.Add(key);
        await _repository.SaveChangesAsync(cancellationToken);

        return key;
    }

    public async Task<bool> RevokeAsync(SigningKeyId id, CancellationToken cancellationToken = default)
    {
        var key = await GetByIdAsync(id, cancellationToken);
        if (key is null)
            return false;
        
        key.Revoke(_time.GetUtcNow().DateTime);
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