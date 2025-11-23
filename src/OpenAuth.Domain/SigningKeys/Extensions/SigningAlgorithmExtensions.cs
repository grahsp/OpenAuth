using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Domain.SigningKeys.Extensions;

public static class SigningAlgorithmExtensions
{
    public static KeyType ToKeyType(this SigningAlgorithm algorithm)
        => algorithm switch
        {
            SigningAlgorithm.RS256 => KeyType.RSA,
            SigningAlgorithm.HS256 => KeyType.HMAC,
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };
    

}