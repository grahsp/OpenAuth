using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.SigningKeys.Persistence;

public class SigningKeyRepository : ISigningKeyRepository
{
    public SigningKeyRepository(AppDbContext context)
    {
        _context = context;
    }

    private readonly AppDbContext _context;
    
    public async Task<SigningKey?> GetByIdAsync(SigningKeyId id, CancellationToken cancellationToken = default)
        => await _context.SigningKeys
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(SigningKey key)
        => _context.SigningKeys.Add(key);

    public void Remove(SigningKey key)
        => _context.SigningKeys.Remove(key);
    
    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}