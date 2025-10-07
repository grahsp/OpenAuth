using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(ClientId id, CancellationToken ct = default);

    void Add(Client client);
    void Remove(Client client);
    Task SaveChangesAsync(CancellationToken ct = default);
}