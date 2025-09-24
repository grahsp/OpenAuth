using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Domain.Entities;

public class SigningKey
{
    private SigningKey() { }
    
    public SigningKey(SigningAlgorithm algorithm, Key key, DateTime createdAt, DateTime expiresAt)
    {
        Id = SigningKeyId.New();
        Algorithm = algorithm;
        Key = key;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }
    

    public SigningKeyId Id { get; private init; }
    public SigningAlgorithm Algorithm { get; private init; }
    public Key Key { get; private init; } = null!;

    public DateTime CreatedAt { get; private init; }
    public DateTime ExpiresAt { get; private init; }
    public DateTime? RevokedAt { get; private set; }

    

    public bool IsActive(DateTime now)
        => !HasExpired(now) && !IsRevoked();

    public bool HasExpired(DateTime now)
        => ExpiresAt <= now;

    public bool IsRevoked()
        => RevokedAt is not null;
    
    public bool Revoke(DateTime now)
    {
        if (RevokedAt is not null)
            return false;
        
        RevokedAt = now;
        return true;
    }
}