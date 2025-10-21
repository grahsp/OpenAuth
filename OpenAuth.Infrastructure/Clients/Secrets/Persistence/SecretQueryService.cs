using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Secrets;
using OpenAuth.Application.Secrets.Dtos;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Secrets.Mappings;
using OpenAuth.Application.Security.Hashing;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.Clients.Secrets.Persistence;

public class SecretQueryService : ISecretQueryService
{
    private readonly AppDbContext _context;
    private readonly IHasher _hasher;
    private readonly TimeProvider _time;
    
    public SecretQueryService(AppDbContext context, IHasher hasher, TimeProvider time)
    {
        _context = context;
        _hasher = hasher;
        _time = time;
    }


    public async Task<bool> ValidateSecretAsync(
        ClientId clientId,
        string plainSecret,
        CancellationToken ct = default)
    {
        var hashes = await _context.ClientSecrets
            .AsNoTracking()
            .Where(s => s.ClientId == clientId)
            .WhereActive(_time.GetUtcNow())
            .Select(s => s.Hash)
            .ToListAsync(ct);

        return hashes.Any(hash => _hasher.Verify(plainSecret, hash.Value));
    }

    public async Task<IReadOnlyCollection<SecretInfo>> GetActiveSecretsAsync(ClientId clientId, CancellationToken ct = default)
    {
        var secrets = await _context.ClientSecrets
            .AsNoTracking()
            .Where(s => s.ClientId == clientId)
            .WhereActive(_time.GetUtcNow())
            .ToListAsync(ct);

        return secrets.Select(s => s.ToSecretInfo()).ToList();
    }
}