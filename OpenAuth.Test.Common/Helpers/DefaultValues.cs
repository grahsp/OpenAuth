using OpenAuth.Domain.OAuth;

namespace OpenAuth.Test.Common.Helpers;

public static class DefaultValues
{
    public const string ApplicationType = "web";

    public const string ClientName = "test-client";
    public const string ClientId = "855c5e72-8fb6-4dd0-88dd-5d830b7ccc60";
    public const string ClientSecret = "client-secret";

    public const string Code = "random-code";
    public const string GrantType = GrantTypes.AuthorizationCode;
    
    public const string ResponseType = "code";
    public const string Subject = "test-subject";
    
    public const string RedirectUri = "https://example.com/callback";
    public const string Audience = "api";
    public const string Scopes = "read write";

    public const string CodeVerifier = "code-verifier";
    public const string CodeChallengeMethod = "s256";
}