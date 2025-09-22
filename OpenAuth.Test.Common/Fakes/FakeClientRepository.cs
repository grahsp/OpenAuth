using OpenAuth.Application.Clients;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Common.Fakes;

public class FakeClientRepository : IClientRepository
{
    public bool Saved { get; private set; }
    private readonly Dictionary<ClientId, Client> _store = new();

    public Task<Client?> GetByIdAsync(ClientId id, CancellationToken ct = default)
        => Task.FromResult(_store.GetValueOrDefault(id));

    public Task<Client?> GetByNameAsync(string name, CancellationToken ct = default)
        => Task.FromResult(_store.Values.FirstOrDefault(c => c.Name == name));
    
    public Task<Client?> GetBySecretIdAsync(SecretId id, CancellationToken ct = default)
        => Task.FromResult(_store.Values.FirstOrDefault(c => c.Secrets.Any(s => s.Id == id)));

    public Task<ClientSecret?> GetSecretAsync(SecretId id, CancellationToken cancellationToken = default)
    {
        var client = _store.Values.FirstOrDefault(c => c.Secrets.Any(s => s.Id == id));
        return Task.FromResult(client?.Secrets.FirstOrDefault(s => s.Id == id));
    }

    public void Add(Client client)
        => _store[client.Id] = client;

    public void Remove(Client client)
        => _store.Remove(client.Id);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        Saved = true;
        return Task.CompletedTask;
    }

    public IReadOnlyCollection<Client> Clients
        => _store.Values.ToList();
}