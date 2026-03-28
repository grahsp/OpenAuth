using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class SecretBuilder
{
    private SecretId? _id;
    private ClientId? _clientId;
    private SecretHash? _hash;
    private DateTimeOffset? _createdAt;
    private TimeSpan? _lifetime;

    public SecretBuilder WithId()
    {
        _id = SecretId.New();
        return this;
    }
    
    public SecretBuilder WithId(SecretId id)
    {
        _id = id;
        return this;
    }

    public SecretBuilder WithClientId()
    {
        _clientId = ClientId.New();
        return this;
    }

    public SecretBuilder WithClientId(ClientId clientId)
    {
        _clientId = clientId;
        return this;
    }
    
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
        var id = _id ?? SecretId.New();
        var clientId = _clientId ?? ClientId.New();
        var hash = _hash
                   ?? new SecretHash("$2a$10$abcdefghijklmnopqrstuv1234567890123456789012345678");
        var createdAt = _createdAt
                        ?? new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var lifetime = _lifetime
                       ?? TimeSpan.FromDays(1);
        
        return Secret.Create(id, clientId, hash, createdAt, lifetime);
    }
}