using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Clients;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;
    public ClientRepository(AppDbContext context) => _context = context;
    
    public Task<Client?> GetByIdAsync(ClientId id, CancellationToken cancellationToken = default)
        => _context.Clients
            .Include(x => x.Secrets)
            .Include(x => x.SigningKeys)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Client?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => _context.Clients
            .Include(x => x.Secrets)
            .Include(x => x.SigningKeys)
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

    public Task<Client?> GetBySecretIdAsync(SecretId id, CancellationToken cancellationToken = default)
        => _context.ClientSecrets
            .Include(x => x.Client)
            .Select(x => x.Client)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<ClientSecret?> GetSecretAsync(SecretId id, CancellationToken cancellationToken = default)
        => _context.ClientSecrets
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(Client client)
        => _context.Clients.Add(client);

    public void Remove(Client client)
        => _context.Clients.Remove(client);
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}