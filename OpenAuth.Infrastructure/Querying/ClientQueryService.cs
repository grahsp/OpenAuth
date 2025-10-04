using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Dtos;
using OpenAuth.Application.Queries;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Infrastructure.Persistence.QuerySpecifications;

namespace OpenAuth.Infrastructure.Querying;

public class ClientQueryService : IClientQueryService
{
    private readonly AppDbContext _context;
    
    public ClientQueryService(AppDbContext context)
    {
        _context = context;
    }


    public Task<ClientInfo?> GetByIdAsync(ClientId id, CancellationToken ct = default)
        => _context.Clients
            .Where(x => x.Id == id)
            .ToClientInfo()
            .SingleOrDefaultAsync(ct);

    public Task<ClientInfo?> GetByNameAsync(ClientName name, CancellationToken ct = default)
        => _context.Clients
            .Where(x => x.Name == name)
            .ToClientInfo()
            .SingleOrDefaultAsync(ct);

    public Task<ClientDetails?> GetDetailsAsync(ClientId id, CancellationToken ct = default)
        => _context.Clients
            .Where(x => x.Id == id)
            .ToClientDetails()
            .SingleOrDefaultAsync(ct);

    public async Task<PagedResult<ClientInfo>> GetPagedAsync(int page, int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.Clients
            .OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(ct);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToClientInfo()
            .ToListAsync(ct);

        return new PagedResult<ClientInfo>(items, totalCount, page, pageSize);
    }

    public Task<bool> ExistsAsync(ClientId id, CancellationToken ct = default)
        => _context.Clients.AnyAsync(x => x.Id == id, ct);

    public Task<bool> NameExistsAsync(ClientName name, CancellationToken ct = default)
        => _context.Clients.AnyAsync(x => x.Name == name, ct);
}