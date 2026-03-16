using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Infrastructure.Security.Extensions;

public static class SigningAlgorithmExtensions
{
    public static string ToSecurityString(this SigningAlgorithm algorithm)
        => algorithm switch
        {
            SigningAlgorithm.RS256 => SecurityAlgorithms.RsaSha256,
            SigningAlgorithm.HS256 => SecurityAlgorithms.HmacSha256,
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };
}