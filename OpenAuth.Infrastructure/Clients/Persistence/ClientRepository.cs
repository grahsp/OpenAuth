using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Clients;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.Clients.Persistence;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;
    public ClientRepository(AppDbContext context) => _context = context;
    
    public Task<Client?> GetByIdAsync(ClientId id, CancellationToken ct = default)
        => _context.Clients
            .Include(c => c.Audiences)
            .Include(c => c.Secrets)
            .SingleOrDefaultAsync(c => c.Id == id, ct);

    public void Add(Client client)
        => _context.Clients.Add(client);

    public void Remove(Client client)
        => _context.Clients.Remove(client);
    
    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}