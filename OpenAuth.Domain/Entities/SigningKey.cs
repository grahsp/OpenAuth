using System.Linq.Expressions;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Domain.Entities;

public class SigningKey
{
    private SigningKey() { }
    
    public SigningKey(SigningAlgorithm algorithm, Key key, DateTime? expiresAt = null)
    {
        Id = SigningKeyId.New();
        Algorithm = algorithm;
        Key = key;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(30);
    }
    
    public SigningKeyId Id { get; init; }
    public SigningAlgorithm Algorithm { get; private init; }
    public Key Key { get; private init; } = null!;

    public DateTime CreatedAt { get; private init; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }


    public static Expression<Func<SigningKey, bool>> IsActiveExpression
        => k => k.RevokedAt == null && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow);
    
    public bool IsActive() => IsActiveExpression.Compile().Invoke(this);
    
    public void Revoke() =>
        RevokedAt ??= DateTime.UtcNow;
}