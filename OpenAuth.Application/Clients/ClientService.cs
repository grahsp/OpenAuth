using OpenAuth.Application.Dtos;
using OpenAuth.Application.Dtos.Mappings;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Clients;

public class ClientService : IClientService
{
    private readonly IClientRepository _repository;
    private readonly IClientFactory _clientFactory;
    private readonly TimeProvider _time;
    
    public ClientService(IClientRepository repository, IClientFactory clientFactory, TimeProvider time)
    {
        _repository = repository;
        _clientFactory = clientFactory;
        _time = time;
    }
    
    
    public async Task<ClientInfo> RegisterAsync(ClientName name, CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.Create(name);
        
        _repository.Add(client);
        await _repository.SaveChangesAsync(cancellationToken);

        return client.ToClientInfo();
    }
    
    public async Task<ClientInfo> RenameAsync(ClientId id, ClientName name, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.Rename(name, _time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);

        return client.ToClientInfo();
    }
    
    public async Task DeleteAsync(ClientId id, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Client not found.");;

        _repository.Remove(client);
        await _repository.SaveChangesAsync(cancellationToken);
    }
    
    
    public async Task EnableAsync(ClientId id, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.Enable(_time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DisableAsync(ClientId id, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.Disable(_time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // TODO: Replace nullable Client with explicit result type
    public async Task<Client?> TryAddAudienceAsync(ClientId id, Audience audience,
        CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken);
        if (client is null)
            return null;

        var added = client.TryAddAudience(audience, _time.GetUtcNow());
        if (!added)
            return null;
        
        await _repository.SaveChangesAsync(cancellationToken);
        return client;
    }
    
    public async Task<Client?> TryRemoveAudienceAsync(ClientId id, Audience audience, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken);
        if (client is null)
            return null;

        var removed = client.TryRemoveAudience(audience, _time.GetUtcNow());
        if (!removed)
            return null;
        
        await _repository.SaveChangesAsync(cancellationToken);
        return client;
    }

    public async Task<Client> SetScopesAsync(ClientId id, Audience audience, IEnumerable<Scope> scopes, CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Client not found.");
        
        client.SetScopes(audience, scopes, _time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);

        return client;
    }
    
    public async Task<Client> GrantScopesAsync(ClientId id, Audience audience, IEnumerable<Scope> scopes,
        CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.GrantScopes(audience, scopes, _time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);

        return client;
    }

    public async Task<Client> RevokeScopesAsync(ClientId id, Audience audience, IEnumerable<Scope> scopes,
        CancellationToken cancellationToken = default)
    {
        var client = await _repository.GetByIdAsync(id, cancellationToken)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.RevokeScopes(audience, scopes, _time.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);

        return client;
    }
}