using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.AuthorizationGrants.Enums;

namespace OpenAuth.Domain.AuthorizationGrants.ValueObjects;

public sealed record Pkce
{
    public string CodeChallenge { get; }
    public CodeChallengeMethod CodeChallengeMethod { get; }
    
    private Pkce(string codeChallenge, CodeChallengeMethod method)
    {
        CodeChallenge = codeChallenge;
        CodeChallengeMethod = method;
    }

    public static Pkce Create(string codeChallenge, CodeChallengeMethod method)
        => new(codeChallenge, method);

    public bool Matches(string? codeVerifier)
    {
        if (string.IsNullOrWhiteSpace(codeVerifier))
            return false;
        
        return ComputeChallenge(codeVerifier, CodeChallengeMethod) == CodeChallenge;    
    }

    private static string ComputeChallenge(string codeVerifier, CodeChallengeMethod method)
        => method switch
        {
            CodeChallengeMethod.Plain => codeVerifier,
            CodeChallengeMethod.S256 => ComputeS256(codeVerifier),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
        };

    private static string ComputeS256(string codeVerifier)
    {
        var bytes = Encoding.UTF8.GetBytes(codeVerifier);
        var hash = SHA256.HashData(bytes);
        var base64 = Base64UrlEncoder.Encode(hash);

        return base64;
    }
}