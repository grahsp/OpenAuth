using OpenAuth.Application.Clients;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Common.Fakes;

public class FakeClientSecretFactory : IClientSecretFactory
{
    private int _counter = 0;

    public SecretCreationResult Create(DateTime? expiresAt = null)
    {
        // Make deterministic secrets (increment counter)
        var plain = $"plain-secret-{++_counter}";
        var hash = new SecretHash($"hash:{plain}");
        var secret = new ClientSecret(hash, expiresAt);
        
        return new SecretCreationResult(secret, plain);
    }
}