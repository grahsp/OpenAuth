using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Clients.Secrets;

public sealed class Secret
{
    public SecretId Id { get; private init; }
    
    public Client Client { get; private init; }
    public ClientId ClientId { get; private init; }
    
    public SecretHash Hash { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset ExpiresAt { get; private init; }
    public DateTimeOffset? RevokedAt { get; private set; }
    
    private Secret() { }

    private Secret(SecretId id, ClientId clientId, SecretHash hash, DateTimeOffset utcNow, TimeSpan lifetime)
    {
        Id = id;
        ClientId = clientId;
        Hash = hash;
        CreatedAt = utcNow;
        ExpiresAt = utcNow.Add(lifetime);
    }


    internal static Secret Create(ClientId clientId, SecretHash hash, DateTimeOffset createdAt, TimeSpan lifetime)
        => Create(SecretId.New(), clientId, hash, createdAt, lifetime);

    internal static Secret Create(SecretId id, ClientId clientId, SecretHash hash, DateTimeOffset createdAt, TimeSpan lifetime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(lifetime, TimeSpan.Zero, nameof(lifetime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(lifetime, TimeSpan.FromDays(365), nameof(lifetime));
        
        return new Secret(id, clientId, hash, createdAt, lifetime);
    }
    
    public bool IsActive(DateTimeOffset utcNow) =>
        RevokedAt is null && ExpiresAt > utcNow;
    
    public void Revoke(DateTimeOffset utcNow) =>
        RevokedAt ??= utcNow;
}