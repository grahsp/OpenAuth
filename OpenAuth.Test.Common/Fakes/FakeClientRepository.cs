using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Common.Fakes;

public class FakeClientRepository : IClientRepository
{
    private readonly Dictionary<ClientId, Client> _store = new();

    public Task<Client?> GetByIdAsync(ClientId id, CancellationToken ct = default) =>
        Task.FromResult(_store.GetValueOrDefault(id));

    public Task<Client?> GetByNameAsync(string name, CancellationToken ct = default) =>
        Task.FromResult(_store.Values.FirstOrDefault(c => c.Name == name));

    public void Add(Client client) =>
        _store[client.Id] = client;

    public void Remove(Client client) =>
        _store.Remove(client.Id);

    public IReadOnlyCollection<Client> Clients => _store.Values.ToList();
}