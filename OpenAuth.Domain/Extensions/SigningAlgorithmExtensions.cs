using OpenAuth.Domain.Enums;

namespace OpenAuth.Domain.Extensions;

public static class SigningAlgorithmExtensions
{
    public static KeyType ToKeyType(this SigningAlgorithm algorithm)
        => algorithm switch
        {
            SigningAlgorithm.RS256 => KeyType.RSA,
            SigningAlgorithm.HM256 => KeyType.HMAC,
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };
    

}