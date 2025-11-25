using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Extensions;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ValueObjects;
using TokenContext = OpenAuth.Application.Tokens.Dtos.TokenContext;

namespace OpenAuth.Application.Tokens.Flows;

public class AuthorizationCodeProcessor : TokenRequestProcessor<AuthorizationCodeTokenCommand>
{
    public override GrantType GrantType => GrantType.AuthorizationCode;

    private readonly IAuthorizationGrantStore _grantStore;
    private readonly IAuthorizationCodeValidator _validator;
    
    public AuthorizationCodeProcessor(IAuthorizationGrantStore grantStore, IAuthorizationCodeValidator validator)
    {
        _grantStore = grantStore;
        _validator = validator;
    }
    
    protected override async Task<TokenContext> ProcessAsync(AuthorizationCodeTokenCommand command, ClientTokenData tokenData, CancellationToken ct = default)
    {
        var authorizationGrant = await _grantStore.GetAsync(command.Code)
                                 ?? throw new InvalidGrantException("Invalid or unknown authorization code.");

        var result = await _validator.ValidateAsync(command, tokenData, authorizationGrant, ct);

        authorizationGrant.Consume();
        await _grantStore.RemoveAsync(authorizationGrant.Code);

        var oidcContext = CreateOidcContext(authorizationGrant, result.OidcScopes);
        
        return new TokenContext(
            authorizationGrant.ClientId.ToString(),
            authorizationGrant.Subject,
            result.AudienceName.Value,
            result.ApiScopes,
            oidcContext
        );
    }

    private static OidcContext? CreateOidcContext(AuthorizationGrant authorizationGrant, ScopeCollection oidcScopes)
    {
        if (!oidcScopes.ContainsOpenIdScope())
            return null;

        var nonce = authorizationGrant.Nonce
                    ?? throw new InvalidOperationException("Nonce was expected to not be null after validation.");
        var authTime = (int)authorizationGrant.CreatedAt.ToUnixTimeSeconds();
        
        return new OidcContext(
            nonce,
            authTime,
            oidcScopes
        );
    }
}