using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Mappings;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.Factories;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Services;

public sealed record RegisteredClientResponse(Client Client, string? ClientSecret);

public class ClientService : IClientService
{
    private readonly IClientRepository _repository;
    private readonly IClientFactory _clientFactory;
    private readonly IClientConfigurationFactory _configurationFactory;
    private readonly TimeProvider _time;
    
    public ClientService(
        IClientRepository repository,
        IClientFactory clientFactory,
        IClientConfigurationFactory configurationFactory,
        TimeProvider time)
    {
        _repository = repository;
        _clientFactory = clientFactory;
        _configurationFactory = configurationFactory;
        _time = time;
    }
    
    
    public async Task<RegisteredClientResponse> RegisterAsync(RegisterClientCommand cmd, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        
        var clientConfiguration = _configurationFactory.Create(cmd);
        var client = _clientFactory.Create(clientConfiguration, out var clientSecret);
        
        _repository.Add(client);
        await _repository.SaveChangesAsync(ct);
        
        var response = new RegisteredClientResponse(client, clientSecret);
        return response;
    }
    
    public async Task<ClientInfo> RenameAsync(ClientId id, ClientName name, CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.Rename(name, _time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);

        return client.ToClientInfo();
    }
    
    
    public async Task<ClientDetails> SetGrantTypesAsync(ClientId id, IEnumerable<GrantType> grantTypes,
        CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
                     ?? throw new InvalidOperationException("Client not found.");
        
        client.SetGrantTypes(grantTypes, _time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);
        
        return client.ToClientDetails();
    }

    public async Task<ClientDetails> AddGrantTypeAsync(ClientId id, GrantType grantType,
        CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
                     ?? throw new InvalidOperationException("Client not found.");

        var grantTypes = client.AllowedGrantTypes
            .Append(grantType);
        
        client.SetGrantTypes(grantTypes, _time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);
        
        return client.ToClientDetails();
    }
    
    public async Task<ClientDetails> RemoveGrantTypeAsync(ClientId id, GrantType grantType,
        CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
                     ?? throw new InvalidOperationException("Client not found.");
        
        var grantTypes = client.AllowedGrantTypes
            .Where(r => r != grantType);
        
        client.SetGrantTypes(grantTypes, _time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);
        
        return client.ToClientDetails();
    }


    public async Task<ClientDetails> SetRedirectUrisAsync(ClientId id, IEnumerable<RedirectUri> redirectUris,
        CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Client not found.");
        
        client.SetRedirectUris(redirectUris, _time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);
        
        return client.ToClientDetails();
    }

    public async Task<ClientDetails> AddRedirectUriAsync(ClientId id, RedirectUri redirectUri,
        CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Client not found.");

        var redirectUris = client.RedirectUris
            .Append(redirectUri);
        
        client.SetRedirectUris(redirectUris, _time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);
        
        return client.ToClientDetails();
    }
    
    public async Task<ClientDetails> RemoveRedirectUriAsync(ClientId id, RedirectUri redirectUri,
        CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Client not found.");
        
        var redirectUris = client.RedirectUris
            .Where(r => r != redirectUri);
        
        client.SetRedirectUris(redirectUris, _time.GetUtcNow());
        await _repository.SaveChangesAsync(ct);
        
        return client.ToClientDetails();
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