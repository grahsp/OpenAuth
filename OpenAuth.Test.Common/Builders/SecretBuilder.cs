using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class SecretBuilder
{
    private SecretHash? _hash;
    private DateTimeOffset? _createdAt;
    private TimeSpan? _lifetime;
    
    public SecretBuilder WithHash(string hash)
    {
        _hash = new SecretHash(hash);
        return this;
    }
    
    public SecretBuilder WithHash(SecretHash hash)
    {
        _hash = hash;
        return this;
    }
    
    public SecretBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }
    
    public SecretBuilder WithLifetime(TimeSpan lifetime)
    {
        _lifetime = lifetime;
        return this;
    }

    public Secret Build()
    {
        var hash = _hash
                   ?? new SecretHash("$2a$10$abcdefghijklmnopqrstuv1234567890123456789012345678");
        var createdAt = _createdAt
                        ?? new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var lifetime = _lifetime
                       ?? TimeSpan.FromDays(1);
        
        return Secret.Create(hash, createdAt, lifetime);
    }
}