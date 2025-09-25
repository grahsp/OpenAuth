using System.Security.Cryptography;
using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.Security.Keys;

public class RsaKeyMaterialGenerator : IKeyMaterialGenerator
{
    private const int MinSize = 2048, DefaultSize = 4096, MaxSize = 8192;
    private readonly int _size;
    
    public const KeyType TargetKeyType = KeyType.RSA;
    public IReadOnlyCollection<SigningAlgorithm> SupportedAlgorithms
        => [SigningAlgorithm.RS256];
    
    public RsaKeyMaterialGenerator(int size = DefaultSize)
    {
        if (size is < MinSize or > MaxSize)
            throw new ArgumentOutOfRangeException(nameof(size), $"RSA key size must be between { MinSize } and { MaxSize }.");
        
        _size = size;
    }
    
    
    public KeyMaterial Create(SigningAlgorithm algorithm)
    {
        using var rsa = RSA.Create(_size);
        var key = new Key(ExportPrivateKeyPem(rsa));

        return new KeyMaterial(key, algorithm, TargetKeyType);
    }
    
    private static string ExportPrivateKeyPem(RSA rsa)
    {
        var key = PemEncoding.Write("PRIVATE KEY",rsa.ExportPkcs8PrivateKey());
        return new string(key);
    }
}