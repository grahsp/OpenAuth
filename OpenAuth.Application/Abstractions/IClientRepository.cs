using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Abstractions;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(ClientId id, CancellationToken cancellationToken = default);
    Task<Client?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    void Add(Client client);
    void Remove(Client client);
}