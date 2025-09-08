using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Clients;

public class ClientService : IClientService
{
    private readonly IClientRepository _repository;
    private readonly IClientSecretFactory _secretFactory;
    private readonly ISigningKeyFactory _signingKeyFactory;
    
    public ClientService(IClientRepository repository, IClientSecretFactory secretFactory, ISigningKeyFactory signingKeyFactory)
    {
        _repository = repository;
        _secretFactory = secretFactory;
        _signingKeyFactory = signingKeyFactory;
    }
    
    public async Task<Client?> GetByIdAsync(ClientId id, CancellationToken cancellationToken = default) =>
        await _repository.GetByIdAsync(id, cancellationToken);
    
    public async Task<Client?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await _repository.GetByNameAsync(name, cancellationToken);

    public async Task<Client> RegisterAsync(string name, CancellationToken cancellationToken = default)
    {
        var client = new Client(name);
        _repository.Add(client);
        await _repository.SaveChangesAsync(cancellationToken);

        return client;
    }
    
    public async Task<Client> RenameAsync(ClientId id, string name, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.Rename(name);
        await _repository.SaveChangesAsync(cancellationToken);

        return client;
    }
    
    public async Task<bool> DeleteAsync(ClientId id, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken);
        if (client is null)
            return false;

        _repository.Remove(client);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
    
    
    public async Task<Client> EnableAsync(ClientId id, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.Enable();
        await _repository.SaveChangesAsync(cancellationToken);

        return client;
    }
    
    public async Task<Client> DisableAsync(ClientId id, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.Disable();
        await _repository.SaveChangesAsync(cancellationToken);

        return client;
    }
    
    
    public async Task<Client> GrantScopesAsync(ClientId id, Audience audience, IEnumerable<Scope> scopes,
        CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.GrantScopes(audience, scopes);
        await _repository.SaveChangesAsync(cancellationToken);

        return client;
    }

    public async Task<Client> RevokeScopesAsync(ClientId id, Audience audience, IEnumerable<Scope> scopes,
        CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.RevokeScopes(audience, scopes);
        await _repository.SaveChangesAsync(cancellationToken);

        return client;
    }

    public async Task<Client> RemoveAudienceAsync(ClientId id, Audience audience, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.RemoveAudience(audience);
        await _repository.SaveChangesAsync(cancellationToken);

        return client;
    }


    public async Task<string> AddSecretAsync(ClientId id, DateTime? expiresAt = null, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        var (secret, plain) = _secretFactory.Create(expiresAt);
        client.AddSecret(secret);

        await _repository.SaveChangesAsync(cancellationToken);
        return plain;
    }

    public async Task<bool> RevokeSecretAsync(ClientId clientId, SecretId secretId, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return false;
        
        client.RevokeSecret(secretId);
        await _repository.SaveChangesAsync(cancellationToken);
        
        return true;
    }

    public async Task<bool> RemoveSecretAsync(ClientId clientId, SecretId secretId, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return false;
        
        client.RemoveSecret(secretId);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
    
    
    public async Task<SigningKey> AddSigningKeyAsync(ClientId id, SigningAlgorithm algorithm, DateTime? expiresAt = null, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");

        var signingKey = _signingKeyFactory.Create(algorithm, expiresAt);
        client.AddSigningKey(signingKey);

        await _repository.SaveChangesAsync(cancellationToken);
        return signingKey;
    }

    public async Task<bool> RevokeSigningKeyAsync(ClientId clientId, SigningKeyId keyId, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return false;
        
        client.RevokeSigningKey(keyId);
        await _repository.SaveChangesAsync(cancellationToken);
        
        return true;
    }

    public async Task<bool> RemoveSigningKeyAsync(ClientId clientId, SigningKeyId keyId, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(clientId, cancellationToken);
        if (client is null)
            return false;
        
        client.RemoveSigningKey(keyId);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}