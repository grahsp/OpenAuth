using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Clients;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(ClientId id, CancellationToken ct = default);

    void Add(Client client);
    void Remove(Client client);
    Task SaveChangesAsync(CancellationToken ct = default);
}