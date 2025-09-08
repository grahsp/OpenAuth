using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Clients;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(ClientId id, CancellationToken cancellationToken = default);
    Task<Client?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    void Add(Client client);
    void Remove(Client client);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}