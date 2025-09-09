using OpenAuth.Application.Security.Secrets;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Clients;

public class ClientSecretFactory : IClientSecretFactory
{
    public ClientSecretFactory(ISecretGenerator generator, ISecretHasher hasher)
    {
        _generator = generator;
        _hasher = hasher;
    }

    private readonly ISecretGenerator _generator;
    private readonly ISecretHasher _hasher;

    public SecretCreationResult Create(DateTime? expiresAt = null)
    {
        var plain = _generator.Generate();
        var hash = _hasher.Hash(plain);
        var secret = new ClientSecret(hash, expiresAt);

        return new SecretCreationResult(secret, plain);
    }
}