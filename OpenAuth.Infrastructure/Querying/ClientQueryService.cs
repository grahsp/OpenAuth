using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Dtos;
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


    public Task<ClientInfo?> GetByIdAsync(ClientId id, CancellationToken ct = default)
        => _context.Clients
            .Where(x => x.Id == id)
            .Select(c => new ClientInfo(
                c.Id,
                c.Name,
                c.CreatedAt,
                c.UpdatedAt
            )).SingleOrDefaultAsync(ct);

    public Task<ClientInfo?> GetByNameAsync(ClientName name, CancellationToken ct = default)
        => _context.Clients
            .Where(x => x.Name == name)
            .Select(c => new ClientInfo(
                c.Id,
                c.Name,
                c.CreatedAt,
                c.UpdatedAt
            )).SingleOrDefaultAsync(ct);

    public Task<ClientDetails?> GetDetailsAsync(ClientId id, CancellationToken ct = default)
        => _context.Clients
            .Where(x => x.Id == id)
            .Select(c => new ClientDetails(
                c.Id,
                c.Name,
                c.CreatedAt,
                c.UpdatedAt,
                c.Secrets
                    .OrderByDescending(s => s.CreatedAt)
                    .Select(s => new SecretInfo(
                        s.Id,
                        s.CreatedAt,
                        s.ExpiresAt,
                        s.RevokedAt
                    )),
                c.Audiences.Select(a => new AudienceInfo(
                    a.Value,
                    a.Scopes
                        .Select(s => s.Value)))
            )).SingleOrDefaultAsync(ct);

    public async Task<PagedResult<ClientInfo>> GetPagedAsync(int page, int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.Clients
            .OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(ct);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClientInfo(
                c.Id,
                c.Name,
                c.CreatedAt,
                c.UpdatedAt
            )).ToListAsync(ct);

        return new PagedResult<ClientInfo>(items, totalCount, page, pageSize);
    }

    public Task<bool> ExistsAsync(ClientId id, CancellationToken ct = default)
        => _context.Clients.AnyAsync(x => x.Id == id, ct);

    public Task<bool> NameExistsAsync(ClientName name, CancellationToken ct = default)
        => _context.Clients.AnyAsync(x => x.Name == name, ct);
}