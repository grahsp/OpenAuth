using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.Oidc;
using OpenAuth.Application.Tokens;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.SigningKeys;
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

    public static AuthorizeCommand CreateValidAuthorizationCommand()
        => new AuthorizeCommandBuilder()
            .WithPkce(CreateValidPkce())
            .WithNonce(DefaultValues.Nonce)
            .Build();

    public static AuthorizationValidationResult CreateValidAuthorizationValidationResult()
    {
        return new AuthorizationValidationResult(
            ClientId.Create(DefaultValues.ClientId),
            ScopeCollection.Parse(DefaultValues.Scopes),
            RedirectUri.Create(DefaultValues.RedirectUri),
            CreateValidPkce(),
            DefaultValues.Nonce
        );
    }

    public static ClientAuthorizationData CreateValidAuthorizationData()
    {
        return new ClientAuthorizationData(
            ClientId.Create(DefaultValues.ClientId),
            true,
            [GrantType.AuthorizationCode],
            [RedirectUri.Create(DefaultValues.RedirectUri)]
        );
    }
    
    public static AuthorizationCodeTokenCommand CreateValidAuthorizationCodeTokenCommand()
    {
        return AuthorizationCodeTokenCommand.Create(
            DefaultValues.Code,
            ClientId.Create(DefaultValues.ClientId),
            RedirectUri.Create(DefaultValues.RedirectUri),
            ScopeCollection.Parse(DefaultValues.Scopes),
            DefaultValues.CodeVerifier,
            DefaultValues.ClientSecret
        );
    }

    public static AuthorizationCodeValidationResult CreateValidAuthorizationCodeValidationResult()
    {
        return new AuthorizationCodeValidationResult(
            AudienceName.Create(DefaultValues.Audience),
            ScopeCollection.Parse(DefaultValues.Scopes),
            ScopeCollection.Parse("")
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

    public static TokenContext CreateValidTokenContext()
    {
        return new TokenContext(
            ScopeCollection.Parse(DefaultValues.Scopes),
            DefaultValues.ClientId,
            DefaultValues.Audience,
            DefaultValues.Subject,
            CreateValidOidcContext()
        );
    }

    public static AccessTokenContext CreateValidAccessTokenContext()
    {
        return new AccessTokenContext(
            DefaultValues.ClientId,
            DefaultValues.Audience,
            DefaultValues.Subject,
            3600,
            ScopeCollection.Parse(DefaultValues.Scopes)
        );
    }

    public static IdTokenContext CreateValidIdTokenContext()
    {
        return new IdTokenContext(
            DefaultValues.ClientId,
            DefaultValues.Subject,
            DefaultValues.Nonce,
            1000,
            3600,
            ScopeCollection.Parse("openid profile")
        );
    }

    public static OidcContext CreateValidOidcContext()
    {
        return new OidcContext(
            3600,
            ScopeCollection.Parse("openid profile"),
            DefaultValues.Nonce
        );
    }

    public static SigningKey CreateValidRsaSigningKey()
        => new SigningKeyBuilder()
            .AsRsa()
            .Build();

    public static SigningKey CreateValidHmacSigningKey()
        => new SigningKeyBuilder()
            .AsHmac()
            .Build();
}