using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.AuthorizationGrant.Enums;

namespace OpenAuth.Domain.AuthorizationGrant.ValueObjects;

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
    {
        var challenge = ComputeChallenge(codeChallenge, method);
        return new Pkce(challenge, method);
    }

    public bool Matches(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;
        
        return ComputeChallenge(code, CodeChallengeMethod) == CodeChallenge;    
    }

    private static string ComputeChallenge(string code, CodeChallengeMethod method)
        => method switch
        {
            CodeChallengeMethod.Plain => code,
            CodeChallengeMethod.S256 => ComputeS256(code),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
        };

    private static string ComputeS256(string code)
    {
        var bytes = Encoding.UTF8.GetBytes(code);
        var hash = SHA256.HashData(bytes);
        var base64 = Base64UrlEncoder.Encode(hash);

        return base64;
    }
}