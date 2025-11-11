using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Mappings;
using OpenAuth.Domain.Clients.Factories;
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
    
    
    public async Task<ClientInfo> RegisterAsync(ClientName name, CancellationToken ct = default)
    {
        var client = _clientFactory.Create(name);
        
        _repository.Add(client);
        await _repository.SaveChangesAsync(ct);

        return client.ToClientInfo();
    }
    
    public async Task<ClientInfo> RenameAsync(ClientId id, ClientName name, CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.Rename(name, _time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);

        return client.ToClientInfo();
    }

    public async Task<ClientDetails> SetAudiencesAsync(ClientId id, IEnumerable<Audience> audiences,
        CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Client not found.");

        client.SetAudiences(audiences, _time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);

        return client.ToClientDetails();
    }

    public async Task<ClientDetails> AddAudienceAsync(ClientId id, Audience audience,
        CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Client not found.");

        var audiences = client.AllowedAudiences
            .Append(audience);
        
        client.SetAudiences(audiences, _time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);

        return client.ToClientDetails();
    }

    public async Task<ClientDetails> RemoveAudienceAsync(ClientId id, AudienceName name,
        CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Client not found.");
        
        var audiences = client.AllowedAudiences
            .Where(a => a.Name != name);
        
        client.SetAudiences(audiences, _time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);
        
        return client.ToClientDetails();
    }
    
    public async Task DeleteAsync(ClientId id, CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
                     ?? throw new InvalidOperationException("Client not found.");

        _repository.Remove(client);
        await _repository.SaveChangesAsync(ct);
    }
    
    
    public async Task EnableAsync(ClientId id, CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.Enable(_time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);
    }
    
    public async Task DisableAsync(ClientId id, CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.Disable(_time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);
    }
}