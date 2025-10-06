using OpenAuth.Application.Dtos;
using OpenAuth.Application.Dtos.Mappings;
using OpenAuth.Application.Security.Keys;
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


    public async Task<SigningKeyInfo> CreateAsync(SigningAlgorithm algorithm, TimeSpan? lifetime = null, CancellationToken ct = default)
    {
        var key = _keyFactory.Create(algorithm, _time.GetUtcNow(), lifetime);
        
        _repository.Add(key);
        await _repository.SaveChangesAsync(ct);

        return key.ToSigningKeyInfo();
    }

    public async Task RevokeAsync(SigningKeyId id, CancellationToken ct = default)
    {
        var key = await _repository.GetByIdAsync(id, ct) ??
                  throw new InvalidOperationException("Signing key not found.");
        
        key.Revoke(_time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);
    }
}