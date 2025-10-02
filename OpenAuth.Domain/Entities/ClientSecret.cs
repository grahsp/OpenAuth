using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Domain.Entities;

public class ClientSecret
{
    private ClientSecret() { }

    private ClientSecret(SecretHash hash, DateTimeOffset utcNow, TimeSpan lifetime)
    {
        Hash = hash;
        CreatedAt = utcNow;
        ExpiresAt = utcNow.Add(lifetime);
    }

    public SecretId Id { get; private init; } = SecretId.New();
    
    public ClientId ClientId { get; private init; }
    public Client Client { get; private init; } = null!;
    
    public SecretHash Hash { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset ExpiresAt { get; private init; }
    public DateTimeOffset? RevokedAt { get; private set; }


    internal static ClientSecret Create(SecretHash hash, DateTimeOffset createdAt, TimeSpan lifetime)
    {
        if (lifetime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Lifetime must be positive.");
        
        if (lifetime > TimeSpan.FromDays(365))
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Lifetime cannot exceed 1 year.");
        
        return new ClientSecret(hash, createdAt, lifetime);
    }
    
    public bool IsActive(DateTimeOffset utcNow) =>
        RevokedAt is null && ExpiresAt > utcNow;
    
    public void Revoke(DateTimeOffset utcNow) =>
        RevokedAt ??= utcNow;
}