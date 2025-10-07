using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Factories;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Mappings;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Services;

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
}