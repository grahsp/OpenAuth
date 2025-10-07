using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Secrets.Dtos;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Security.Hashing;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Secrets.Services;

public class SecretService : ISecretService
{
    private readonly IClientRepository _repository;
    private readonly ISecretGenerator _generator;
    private readonly ISecretHasher _hasher;
    private readonly TimeProvider _time;
    
    public SecretService(IClientRepository repository, ISecretGenerator generator, ISecretHasher hasher, TimeProvider time)
    {
        _repository = repository;
        _generator = generator;
        _hasher = hasher;
        _time = time;
    }


    public async Task<SecretCreationResult> AddSecretAsync(ClientId clientId, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(clientId, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        var plainSecret = _generator.Generate();
        var hashedSecret = _hasher.Hash(plainSecret);
        
        var secretId = client.AddSecret(hashedSecret, _time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);
        
        var secret = client.Secrets.Single(s => s.Id == secretId);
        
        return new SecretCreationResult(
            plainSecret,
            secret.Id,
            secret.CreatedAt,
            secret.ExpiresAt
        );
    }

    public async Task RevokeSecretAsync(ClientId clientId, SecretId secretId, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(clientId, cancellationToken)
            ?? throw new InvalidOperationException("Client not found.");
        
        client.RevokeSecret(secretId, _time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);
    }
}