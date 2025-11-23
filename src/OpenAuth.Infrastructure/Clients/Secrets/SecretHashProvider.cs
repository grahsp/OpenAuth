using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Services;
using OpenAuth.Domain.Services.Dtos;

namespace OpenAuth.Infrastructure.Clients.Secrets;

public class SecretHashProvider : ISecretHashProvider
{
    private readonly ISecretGenerator _generator;
    private readonly IHasher _hasher;
    
    public SecretHashProvider(ISecretGenerator generator, IHasher hasher)
    {
        _generator = generator;
        _hasher = hasher;
    }
    
    public SecretCreationResult Create()
    {
        var plain = _generator.Generate();
        var hash = SecretHash.Create(plain, _hasher);
        
        return new SecretCreationResult(plain, hash);
    }
}