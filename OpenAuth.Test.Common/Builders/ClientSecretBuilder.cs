using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class ClientSecretBuilder
{
    private SecretHash? _hash;
    private DateTimeOffset? _createdAt;
    private TimeSpan? _lifetime;
    
    public ClientSecretBuilder WithHash(string hash)
    {
        _hash = new SecretHash(hash);
        return this;
    }
    
    public ClientSecretBuilder WithHash(SecretHash hash)
    {
        _hash = hash;
        return this;
    }
    
    public ClientSecretBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }
    
    public ClientSecretBuilder WithLifetime(TimeSpan lifetime)
    {
        _lifetime = lifetime;
        return this;
    }

    public ClientSecret Build()
    {
        var hash = _hash
                   ?? new SecretHash("$2a$10$abcdefghijklmnopqrstuv1234567890123456789012345678");
        var createdAt = _createdAt
                        ?? new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var lifetime = _lifetime
                       ?? TimeSpan.FromDays(1);
        
        return ClientSecret.Create(hash, createdAt, lifetime);
    }
}