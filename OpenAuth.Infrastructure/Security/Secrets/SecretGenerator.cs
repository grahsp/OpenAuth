using System.Buffers.Text;
using System.Security.Cryptography;
using OpenAuth.Application.Security.Secrets;

namespace OpenAuth.Infrastructure.Security.Secrets;

public class SecretGenerator : ISecretGenerator
{
    private const int MinBytes = 16, DefaultBytes = 32, MaxBytes = 64;

    public string Generate() => Generate(DefaultBytes);
    
    public string Generate(int byteLength)
    {
        if (byteLength is < MinBytes or > MaxBytes)
            throw new ArgumentOutOfRangeException(nameof(byteLength),
                $"Secret length must be between {MinBytes} and {MaxBytes} bytes.");

        var data = RandomNumberGenerator.GetBytes(byteLength);
        return Base64Url.EncodeToString(data);
    }
}