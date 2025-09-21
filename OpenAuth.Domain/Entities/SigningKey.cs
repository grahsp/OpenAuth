using System.Linq.Expressions;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Domain.Entities;

public class SigningKey
{
    private SigningKey() { }
    
    private SigningKey(SigningAlgorithm algorithm, DateTime? expiresAt)
    {
        KeyId = SigningKeyId.New();
        Algorithm = algorithm;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
    }
    
    public SigningKeyId KeyId { get; init; }
    
    public ClientId ClientId { get; private set; }
    public Client Client { get; private set; } = null!;
    
    public SigningAlgorithm Algorithm { get; private init; }

    public string PublicKey { get; private init; } = null!;
    public string PrivateKey { get; private init; } = null!;

    public DateTime CreatedAt { get; private init; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    
    public static SigningKey CreateSymmetric(SigningAlgorithm algorithm, string key, DateTime? expiresAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        return new SigningKey(algorithm, expiresAt)
        {
            PrivateKey = key
        };
    }

    public static SigningKey CreateAsymmetric(SigningAlgorithm algorithm, string publicKey, string privateKey,
        DateTime? expiresAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publicKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKey);
        
        return new SigningKey(algorithm, expiresAt)
        {
            PublicKey = publicKey,
            PrivateKey = privateKey
        };
    }

    public static Expression<Func<SigningKey, bool>> IsActiveExpression
        => k => k.RevokedAt == null && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow);
    
    public bool IsActive() => IsActiveExpression.Compile().Invoke(this);
    
    public void Revoke() =>
        RevokedAt ??= DateTime.UtcNow;
}