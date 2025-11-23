using OpenAuth.Domain.AuthorizationGrants.ValueObjects;

namespace OpenAuth.Test.Common.Helpers;

public static class PkceHelpers
{
    public static (string verifier, Pkce pkce) Create(
        string verifier = DefaultValues.CodeVerifier,
        string method = DefaultValues.CodeChallengeMethod)
        => (verifier, Pkce.FromVerifier(verifier, method));
}