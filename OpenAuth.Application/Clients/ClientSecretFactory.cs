using OpenAuth.Domain.Abstractions;
using OpenAuth.Domain.Entities;

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

    public (ClientSecret secret, string plain) Create(DateTime? expiresAt = null)
    {
        var plain = _generator.Generate();
        var hash = _hasher.Hash(plain);
        var secret = new ClientSecret(hash, expiresAt);

        return (secret, plain);
    }
}