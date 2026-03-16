using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.Oidc;
using OpenAuth.Application.Tokens;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.OAuth;
using OpenAuth.Domain.Services.Dtos;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Test.Common.Builders;

namespace OpenAuth.Test.Common.Helpers;

public static class TestData
{
    public static Pkce CreateValidPkce(string? codeVerifier = null)
    {
        var verifier = codeVerifier ?? DefaultValues.CodeVerifier;
        return Pkce.FromVerifier(verifier, DefaultValues.CodeChallengeMethod);
    }

    public static AuthorizeCommand CreateValidAuthorizationCommand()
        => new AuthorizeCommandBuilder()
            .WithPkce(CreateValidPkce())
            .WithNonce(DefaultValues.Nonce)
            .Build();

    public static AuthorizationValidationResult CreateValidAuthorizationValidationResult()
    {
        return new AuthorizationValidationResult(
            ClientId.Parse(DefaultValues.ClientId),
            new AudienceIdentifier(DefaultValues.ApiAudience),
            ScopeCollection.Parse(DefaultValues.Scopes),
            RedirectUri.Parse(DefaultValues.RedirectUri),
            CreateValidPkce(),
            DefaultValues.Nonce
        );
    }

    public static ClientAuthorizationData CreateValidAuthorizationData()
    {
        return new ClientAuthorizationData(
            ClientId.Parse(DefaultValues.ClientId),
            true,
            [GrantType.AuthorizationCode],
            [RedirectUri.Parse(DefaultValues.RedirectUri)]
        );
    }
    
    public static AuthorizationCodeTokenCommand CreateValidAuthorizationCodeTokenCommand()
    {
        return AuthorizationCodeTokenCommand.Create(
            DefaultValues.Code,
            ClientId.Parse(DefaultValues.ClientId),
            RedirectUri.Parse(DefaultValues.RedirectUri),
            ScopeCollection.Parse(DefaultValues.Scopes),
            DefaultValues.CodeVerifier,
            DefaultValues.ClientSecret
        );
    }

    public static AuthorizationCodeValidationResult CreateValidAuthorizationCodeValidationResult()
    {
        return new AuthorizationCodeValidationResult(
            ScopeCollection.Parse(DefaultValues.Scopes),
            ScopeCollection.Parse("")
        );
    }

    public static ClientTokenData CreateValidTokenData()
    {
        return new ClientTokenData(
            ClientId.Parse(DefaultValues.ClientId),
            [GrantType.Parse(DefaultValues.GrantType)],
            TimeSpan.FromMinutes(30)
        );
    }

    public static TokenContext CreateValidTokenContext()
    {
        return new TokenContext(
            ScopeCollection.Parse(DefaultValues.Scopes),
            DefaultValues.ClientId,
            new AudienceIdentifier(DefaultValues.ApiAudience),
            DefaultValues.Subject,
            CreateValidOidcContext()
        );
    }

    public static AccessTokenContext CreateValidAccessTokenContext()
    {
        return new AccessTokenContext(
            DefaultValues.ClientId,
            DefaultValues.ApiAudience,
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

    public static JwtDescriptor CreateValidJwtDescriptor()
        => new JwtDescriptor(
            DefaultValues.ApiAudience,
            DefaultValues.Subject,
            3600,
            new Dictionary<string, object> { { "scope", "openid" } });
}