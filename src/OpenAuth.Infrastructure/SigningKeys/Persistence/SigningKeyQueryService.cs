using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.SigningKeys.Persistence;

public class SigningKeyQueryService : ISigningKeyQueryService
{
    private readonly AppDbContext _context;
    private readonly TimeProvider _time;

    public SigningKeyQueryService(AppDbContext context, TimeProvider time)
    {
        _context = context;
        _time = time;
    }
    
    
    public Task<SigningKeyData?> GetCurrentKeyDataAsync(CancellationToken ct = default)
        => _context.SigningKeys
            .AsNoTracking()
            .WhereActive(_time.GetUtcNow())
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new SigningKeyData(
                k.Id,
                k.KeyMaterial.Kty,
                k.KeyMaterial.Alg,
                k.KeyMaterial.Key
            ))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyCollection<SigningKeyData>> GetActiveKeyDataAsync(CancellationToken ct = default)
        => await _context.SigningKeys
            .AsNoTracking()
            .WhereActive(_time.GetUtcNow())
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new SigningKeyData(
                k.Id,
                k.KeyMaterial.Kty,
                k.KeyMaterial.Alg,
                k.KeyMaterial.Key
            ))
            .ToListAsync(ct);

    public Task<SigningKeyInfo?> GetByIdAsync(SigningKeyId id, CancellationToken ct = default)
        => _context.SigningKeys
            .AsNoTracking()
            .Where(k => k.Id == id)
            .Select(k => new SigningKeyInfo(
                k.Id,
                k.KeyMaterial.Kty,
                k.KeyMaterial.Alg,
                k.CreatedAt,
                k.ExpiresAt,
                k.RevokedAt
            ))
            .SingleOrDefaultAsync(ct);

    public async Task<IReadOnlyCollection<SigningKeyInfo>> GetAllAsync(CancellationToken ct = default)
        => await _context.SigningKeys
            .AsNoTracking()
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new SigningKeyInfo(
                k.Id,
                k.KeyMaterial.Kty,
                k.KeyMaterial.Alg,
                k.CreatedAt,
                k.ExpiresAt,
                k.RevokedAt
            ))
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<SigningKeyInfo>> GetActiveAsync(CancellationToken ct = default)
        => await _context.SigningKeys
            .AsNoTracking()
            .WhereActive(_time.GetUtcNow())
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new SigningKeyInfo(
                k.Id,
                k.KeyMaterial.Kty,
                k.KeyMaterial.Alg,
                k.CreatedAt,
                k.ExpiresAt,
                k.RevokedAt
            ))
            .ToListAsync(ct);
}