using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.Repositories;

public class SigningKeyRepository : ISigningKeyRepository
{
    public SigningKeyRepository(AppDbContext context)
    {
        _context = context;
    }

    private readonly AppDbContext _context;
    
    public async Task<SigningKey?> GetByIdAsync(SigningKeyId id, CancellationToken cancellationToken = default)
        => await _context.SigningKeys
            .SingleOrDefaultAsync(x => x.KeyId == id, cancellationToken);

    public async Task<IEnumerable<SigningKey>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.SigningKeys
            .ToListAsync(cancellationToken);

    public void Add(SigningKey key)
        => _context.SigningKeys.Add(key);

    public void Remove(SigningKey key)
        => _context.SigningKeys.Remove(key);
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}