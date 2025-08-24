using OpenAuth.Application.Repositories;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Tests.Stubs;

public class FakeClientRepository : IClientRepository
{
    private readonly Dictionary<ClientId, Client> _store = new();

    public Task<Client?> GetByIdAsync(ClientId id, CancellationToken ct = default) =>
        Task.FromResult(_store.GetValueOrDefault(id));

    public Task<Client?> GetByNameAsync(string name, CancellationToken ct = default) =>
        Task.FromResult(_store.Values.FirstOrDefault(c => c.Name == name));

    public Task AddAsync(Client client, CancellationToken ct = default)
    {
        _store[client.Id] = client;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Client client, CancellationToken ct = default)
    {
        _store.Remove(client.Id);
        return Task.CompletedTask;
    }

    public IReadOnlyCollection<Client> Clients => _store.Values.ToList();
}