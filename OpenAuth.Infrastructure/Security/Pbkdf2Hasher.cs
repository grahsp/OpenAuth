using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using OpenAuth.Domain.Services;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.Security;

public class Pbkdf2Hasher : ISecretHasher
{
    private const string Version = "v1";
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private readonly int _iterations;
    
    public Pbkdf2Hasher(int iterations = 100_000)
    {
        if (iterations < 10_000)
            throw new ArgumentOutOfRangeException(nameof(iterations), "iterations must be greater or equal to 10_000");
        
        _iterations = iterations;
    }
    
    public SecretHash Hash(string plain)
    {
        ArgumentNullException.ThrowIfNull(plain);
        ArgumentException.ThrowIfNullOrWhiteSpace(plain);
        
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = DeriveKey(plain, salt, _iterations, KeySize);

        var encoded = string.Join('$',
            Version,
            _iterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));

        return new SecretHash(encoded);
    }

    public bool Verify(string plain, SecretHash encoded)
    {
        ArgumentNullException.ThrowIfNull(encoded.Value);

        if (string.IsNullOrWhiteSpace(plain))
            return false;

        var parts = encoded.Value.Split('$');
        if (parts is not [Version, _, _, _])
            return false;

        if (!int.TryParse(parts[1], out var iterations))
            return false;

        byte[] salt, expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch { return false; }

        var actual = DeriveKey(plain, salt, iterations, expected.Length);
        return Base64Url.EncodeToString(actual) == Base64Url.EncodeToString(expected);
    }
    
    private byte[] DeriveKey(string input, byte[] salt, int iterations, int length)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(input, salt, iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(length);
    }
}