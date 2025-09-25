using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Domain.Entities;

/// <summary>
/// Represents a cryptographic signing key used to sign tokens.
/// </summary>
public class SigningKey
{
    private SigningKey() { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SigningKey"/> class.
    /// </summary>
    /// <param name="keyMaterial">The data to generate and validate tokens.</param>
    /// <param name="createdAt">The UTC time when the key was created.</param>
    /// <param name="expiresAt">The UTC time when the key expires.</param> 
    public SigningKey(KeyMaterial keyMaterial, DateTime createdAt, DateTime expiresAt)
    {
        Id = SigningKeyId.New();
        KeyMaterial = keyMaterial;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }
    
    /// <summary>
    /// Gets the unique identifier for this signing key (kid).
    /// </summary>
    public SigningKeyId Id { get; private init; }

    /// <summary>
    /// Gets the key material used to generate and validate tokens.
    /// </summary>
    public KeyMaterial KeyMaterial { get; private init; } = null!;

    /// <summary>
    /// Gets the time when the key was created.
    /// </summary>
    public DateTime CreatedAt { get; private init; }
    
    /// <summary>
    /// Gets the time when the key expires.
    /// </summary>
    public DateTime ExpiresAt { get; private init; }
    
    /// <summary>
    /// Gets the time when the key was revoked, if any.
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    
    
    /// <summary>
    /// Determines whether this key is currently active (not revoked and not expired).
    /// </summary>
    public bool IsActive(DateTime now)
        => !HasExpired(now) && !IsRevoked();

    /// <summary>
    /// Determines whether this key has expired at the given point in time.
    /// </summary>
    public bool HasExpired(DateTime now)
        => ExpiresAt <= now;

    /// <summary>
    /// Determines whether this key has been revoked.
    /// </summary>
    public bool IsRevoked()
        => RevokedAt is not null;
    
    /// <summary>
    /// Revokes this key, preventing further use.
    /// </summary>
    /// <param name="now">The time of revocation.</param>
    /// <returns><c>true</c> if the key was successfully revoked; otherwise <c>false</c>.</returns>
    public bool Revoke(DateTime now)
    {
        if (RevokedAt is not null)
            return false;
        
        RevokedAt = now;
        return true;
    }
}