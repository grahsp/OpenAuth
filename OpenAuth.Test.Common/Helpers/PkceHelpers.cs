using OpenAuth.Domain.AuthorizationGrants.ValueObjects;

namespace OpenAuth.Test.Common.Helpers;

public static class PkceHelpers
{
    public static (string verifier, Pkce pkce) Create(string verifier = "code-verifier", string method = "s256")
        => (verifier, Pkce.FromVerifier(verifier, method));
}