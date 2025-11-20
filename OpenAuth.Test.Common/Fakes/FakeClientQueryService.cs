using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Shared.Models;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Fakes;

public class FakeClientQueryService : IClientQueryService
{
    private readonly Dictionary<ClientId, Client> _store = [];

    public void Add(Client client)
        => _store.TryAdd(client.Id, client);

    public void Remove(ClientId id)
        => _store.Remove(id);
    
    public Task<ClientInfo?> GetByIdAsync(ClientId id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ClientInfo?> GetByNameAsync(ClientName name, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ClientDetails?> GetDetailsAsync(ClientId id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ClientTokenData?> GetTokenDataAsync(ClientId id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ClientAuthorizationData?> GetAuthorizationDataAsync(ClientId id, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(id, out var client))
            return Task.FromResult<ClientAuthorizationData?>(null);

        var data = new ClientAuthorizationData(
            client.Id,
            client.IsPublic,
            client.AllowedGrantTypes.ToArray(),
            client.RedirectUris.ToArray()
        );

        return Task.FromResult<ClientAuthorizationData?>(data);
    }

    public Task<PagedResult<ClientInfo>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(ClientId id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> NameExistsAsync(ClientName name, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}