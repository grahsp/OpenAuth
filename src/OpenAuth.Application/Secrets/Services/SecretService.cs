using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.Services;
using OpenAuth.Domain.Services.Dtos;

namespace OpenAuth.Application.Secrets.Services;

public class SecretService : ISecretService
{
    private readonly IClientRepository _repository;
    private readonly ISecretHashProvider _hashProvider;
    private readonly TimeProvider _time;
    
    public SecretService(IClientRepository repository, ISecretHashProvider hashProvider, TimeProvider time)
    {
        _repository = repository;
        _hashProvider = hashProvider;
        _time = time;
    }


    public async Task<SecretCreationResult> AddSecretAsync(ClientId clientId, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(clientId, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        var result = _hashProvider.Create();
        
        client.AddSecret(result.Hash, _time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task RevokeSecretAsync(ClientId clientId, SecretId secretId, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(clientId, cancellationToken)
            ?? throw new InvalidOperationException("Client not found.");
        
        client.RevokeSecret(secretId, _time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);
    }
}