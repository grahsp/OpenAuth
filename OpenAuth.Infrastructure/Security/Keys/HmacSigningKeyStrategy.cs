using System.Security.Cryptography;
using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.Security.Keys;

public class HmacSigningKeyStrategy : ISigningKeyStrategy
{
    private const int MinSize = 32, DefaultSize = 64, MaxSize = 128;
    private readonly int _size;

    public HmacSigningKeyStrategy() : this(DefaultSize) { }

    public HmacSigningKeyStrategy(int size)
    {
        if (size is < MinSize or > MaxSize)
            throw new ArgumentOutOfRangeException(nameof(size), $"HMAC key size must be between { MinSize } and { MaxSize }.");
        
        _size = size;
    }
    
    public SigningAlgorithm Algorithm => SigningAlgorithm.Hmac;

    public SigningKey Create(DateTime? expiresAt = null)
    {
        var key = new Key(GenerateKey());
        
        // TODO: Add passthrough for expiration date
        return new SigningKey(Algorithm, key, DateTime.UtcNow, expiresAt ?? DateTime.MaxValue);
    }

    private string GenerateKey()
    {
        var bytes = new byte[_size];
        RandomNumberGenerator.Fill(bytes);

        return Convert.ToBase64String(bytes);
    }
}