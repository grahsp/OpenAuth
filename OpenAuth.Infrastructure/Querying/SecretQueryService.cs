using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Dtos;
using OpenAuth.Application.Queries;
using OpenAuth.Application.Security.Secrets;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Infrastructure.Persistence.QuerySpecifications;

namespace OpenAuth.Infrastructure.Querying;

public class SecretQueryService : ISecretQueryService
{
    private readonly AppDbContext _context;
    private readonly ISecretHasher _hasher;
    private readonly TimeProvider _time;
    
    public SecretQueryService(AppDbContext context, ISecretHasher hasher, TimeProvider time)
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
            .Where(s => s.ClientId == clientId)
            .WhereActive(_time.GetUtcNow())
            .Select(s => s.Hash)
            .ToListAsync(ct);

        return hashes.Any(hash => _hasher.Verify(plainSecret, hash));
    }
    
    public Task<List<SecretInfo>> GetActiveSecretsAsync(ClientId clientId, CancellationToken ct = default)
        => _context.ClientSecrets
            .WhereActive(_time.GetUtcNow())
            .ToSecretInfo()
            .ToListAsync(ct);
}