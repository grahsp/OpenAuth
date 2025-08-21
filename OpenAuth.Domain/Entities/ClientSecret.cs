using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Domain.Entities;

public class ClientSecret
{
    public ClientSecret() { }

    public ClientSecret(SecretHash hash, DateTime? expiresAt = null)
    {
        Hash = hash;
        ExpiresAt = expiresAt;
    }

    public SecretId Id { get; private init; } = SecretId.New();
    public SecretHash Hash { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    
    
    public bool IsActive() =>
        RevokedAt is null && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);
    
    public void Revoke() =>
        RevokedAt ??= DateTime.UtcNow;
}