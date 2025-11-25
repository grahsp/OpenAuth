using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;

namespace OpenAuth.Test.Common.Helpers;

public static class TestData
{
    public static Audience CreateValidAudience()
        => new(AudienceName.Create(DefaultValues.Audience), ScopeCollection.Parse(DefaultValues.Scopes));

    public static Pkce CreateValidPkce(string? codeVerifier = null)
    {
        var verifier = codeVerifier ?? DefaultValues.CodeVerifier;
        return Pkce.FromVerifier(verifier, DefaultValues.CodeChallengeMethod);
    }

    public static AuthorizationGrant CreateValidAuthorizationGrant()
        => new AuthorizationGrantBuilder().Build();
    
    public static AuthorizationCodeTokenCommand CreateValidAuthorizationCodeTokenCommand()
    {
        return AuthorizationCodeTokenCommand.Create(
            DefaultValues.Code,
            ClientId.Create(DefaultValues.ClientId),
            RedirectUri.Create(DefaultValues.RedirectUri),
            AudienceName.Create(DefaultValues.Audience),
            ScopeCollection.Parse(DefaultValues.Scopes),
            DefaultValues.CodeVerifier,
            DefaultValues.ClientSecret
        );
    }

    public static ClientTokenData CreateValidTokenData()
    {
        return new ClientTokenData(
            ClientId.Create(DefaultValues.ClientId),
            [CreateValidAudience()],
            [GrantType.Create(DefaultValues.GrantType)],
            TimeSpan.FromMinutes(30)
        );
    }
}