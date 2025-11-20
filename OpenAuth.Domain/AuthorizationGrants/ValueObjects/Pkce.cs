using System.Diagnostics.CodeAnalysis;
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

    public static Pkce Create(string codeChallenge, CodeChallengeMethod codeChallengeMethod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codeChallenge);
        return new Pkce(codeChallenge, codeChallengeMethod);
    }
    
    public static Pkce? Parse(string? codeChallenge, string? codeChallengeMethod)
    {
        if (string.IsNullOrWhiteSpace(codeChallenge) || string.IsNullOrWhiteSpace(codeChallengeMethod))
            return null;
        
        var method = Enum.Parse<CodeChallengeMethod>(codeChallengeMethod, true);
        return new Pkce(codeChallenge, method);
    }

    public static Pkce FromVerifier(string verifier, string codeChallengeMethod)
    {
        if (!Enum.TryParse<CodeChallengeMethod>(codeChallengeMethod!, true, out var method))
            throw new ArgumentException($"Invalid code challenge method: {codeChallengeMethod}", nameof(codeChallengeMethod));
                
        return Create(ComputeChallenge(verifier!, method), method);
    }

    public bool Matches(string? codeVerifier)
    {
        if (string.IsNullOrWhiteSpace(codeVerifier))
            return false;
        
        return ComputeChallenge(codeVerifier, CodeChallengeMethod) == CodeChallenge;    
    }

    public static string ComputeChallenge(string codeVerifier, CodeChallengeMethod method)
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