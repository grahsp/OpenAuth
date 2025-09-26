using System.Security.Cryptography;
using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.Security.Keys;

public class HmacKeyMaterialGenerator : IKeyMaterialGenerator
{
    private const int MinSize = 32, DefaultSize = 64, MaxSize = 128;
    private readonly int _size;

    public KeyType KeyType => KeyType.HMAC;

    public HmacKeyMaterialGenerator(int size = DefaultSize)
    {
        if (size is < MinSize or > MaxSize)
            throw new ArgumentOutOfRangeException(nameof(size), $"HMAC key size must be between { MinSize } and { MaxSize }.");
        
        _size = size;
    }

    public KeyMaterial Create(SigningAlgorithm algorithm)
    {
        var key = new Key(GenerateKey());
        return new KeyMaterial(key, algorithm, KeyType);
    }

    private string GenerateKey()
    {
        var bytes = new byte[_size];
        RandomNumberGenerator.Fill(bytes);

        return Convert.ToBase64String(bytes);
    }
}