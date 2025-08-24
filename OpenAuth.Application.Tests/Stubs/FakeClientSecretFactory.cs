using OpenAuth.Domain.Abstractions;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Tests.Stubs;

public class FakeClientSecretFactory : IClientSecretFactory
{
    private int _counter = 0;

    public (ClientSecret secret, string plain) Create(DateTime? expiresAt = null)
    {
        // Make deterministic secrets (increment counter)
        var plain = $"plain-secret-{++_counter}";
        var hash = new SecretHash($"hash:{plain}");
        var secret = new ClientSecret(hash, expiresAt);
        return (secret, plain);
    }
}