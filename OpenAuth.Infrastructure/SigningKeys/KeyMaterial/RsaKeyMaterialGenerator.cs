using System.Security.Cryptography;
using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.SigningKeys.KeyMaterial;

public class RsaKeyMaterialGenerator : IKeyMaterialGenerator
{
    public const int MinSize = 2048, DefaultSize = 4096, MaxSize = 8192;
    private readonly int _size;
    
    public KeyType KeyType => KeyType.RSA;
    
    public RsaKeyMaterialGenerator(int size = DefaultSize)
    {
        if (size is < MinSize or > MaxSize)
            throw new ArgumentOutOfRangeException(nameof(size), $"RSA key size must be between { MinSize } and { MaxSize }.");
        
        if (size % 8 != 0)
            throw new ArgumentOutOfRangeException(nameof(size), $"RSA key size must be a multiple of 8.");
        
        _size = size;
    }
    
    
    public Domain.ValueObjects.KeyMaterial Create(SigningAlgorithm algorithm)
    {
        using var rsa = RSA.Create(_size);
        var key = new Key(ExportPrivateKeyPem(rsa));

        return new Domain.ValueObjects.KeyMaterial(key, algorithm, KeyType);
    }
    
    private static string ExportPrivateKeyPem(RSA rsa)
    {
        var key = PemEncoding.Write("PRIVATE KEY",rsa.ExportPkcs8PrivateKey());
        return new string(key);
    }
}