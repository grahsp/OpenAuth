using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Dtos;
using OpenAuth.Application.Dtos.Mappings;
using OpenAuth.Application.Queries;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.Querying;

public class ClientQueryService : IClientQueryService
{
    private readonly AppDbContext _context;
    
    public ClientQueryService(AppDbContext context)
    {
        _context = context;
    }


    public async Task<ClientInfo?> GetByIdAsync(ClientId id, CancellationToken ct = default)
    {
        var client = await _context.Clients
            .AsNoTracking()
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync(ct);

        return client?.ToClientInfo() ?? null;
    }

    public async Task<ClientInfo?> GetByNameAsync(ClientName name, CancellationToken ct = default)
    {
        var client = await _context.Clients
            .AsNoTracking()
            .Where(x => x.Name == name)
            .SingleOrDefaultAsync(ct);

        return client?.ToClientInfo() ?? null;
    }

    public async Task<ClientDetails?> GetDetailsAsync(ClientId id, CancellationToken ct = default)
    {
        var client = await _context.Clients
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Include(c => c.Audiences)
            .Include(c => c.Secrets)
            .SingleOrDefaultAsync(ct);
        
        return client?.ToClientDetails() ?? null;
    }

    public async Task<PagedResult<ClientInfo>> GetPagedAsync(int page, int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.Clients
            .AsNoTracking()
            .OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(ct);
        
        var clients = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = clients.Select(c => c.ToClientInfo()).ToArray();
        return new PagedResult<ClientInfo>(items, totalCount, page, pageSize);
    }

    public Task<bool> ExistsAsync(ClientId id, CancellationToken ct = default)
        => _context.Clients.AnyAsync(x => x.Id == id, ct);

    public Task<bool> NameExistsAsync(ClientName name, CancellationToken ct = default)
        => _context.Clients.AnyAsync(x => x.Name == name, ct);
}