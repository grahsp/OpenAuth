using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Application.SigningKeys.Factories;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.SigningKeys.Mappings;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Application.SigningKeys.Services;

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