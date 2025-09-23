using System.Security.Cryptography;
using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.Security.Keys;

public class RsaSigningKeyStrategy : ISigningKeyStrategy
{
    private const int MinSize = 2048, DefaultSize = 4096, MaxSize = 8192;
    private readonly int _size;
    
    public RsaSigningKeyStrategy() : this(DefaultSize) { }
    
    public RsaSigningKeyStrategy(int size)
    {
        if (size is < MinSize or > MaxSize)
            throw new ArgumentOutOfRangeException(nameof(size), $"RSA key size must be between { MinSize } and { MaxSize }.");
        
        _size = size;
    }
    
    public SigningAlgorithm Algorithm => SigningAlgorithm.Rsa;
    
    public SigningKey Create(DateTime? expiresAt = null)
    {
        using var rsa = RSA.Create(_size);
        var key = new Key(ExportPrivateKeyPem(rsa));

        return new SigningKey(Algorithm, key, expiresAt);
    }
    
    private static string ExportPrivateKeyPem(RSA rsa)
    {
        var key = PemEncoding.Write("PRIVATE KEY",rsa.ExportPkcs8PrivateKey());
        return new string(key);
    }
}