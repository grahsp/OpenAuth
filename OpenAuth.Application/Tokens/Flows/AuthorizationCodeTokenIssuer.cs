using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;
using TokenContext = OpenAuth.Application.Tokens.Dtos.TokenContext;

namespace OpenAuth.Application.Tokens.Flows;

public class AuthorizationCodeTokenIssuer : TokenIssuerBase<AuthorizationCodeTokenRequest>
{
    private readonly IAuthorizationGrantStore _grantStore;
    
    public override GrantType GrantType => GrantType.AuthorizationCode;

    public AuthorizationCodeTokenIssuer(IAuthorizationGrantStore grantStore)
    {
        _grantStore = grantStore;
    }
    
    protected override async Task<TokenContext> IssueToken(AuthorizationCodeTokenRequest request, CancellationToken ct = default)
    {
        var grant = await _grantStore.GetAsync(request.Code)
                    ?? throw new InvalidOperationException("Invalid authorization code.");
        
        if (grant.Consumed)
            throw new InvalidOperationException("Authorization code has already been used.");

        if (grant.ClientId != request.ClientId)
            throw new InvalidOperationException("Client ID mismatch.");
        
        if (grant.RedirectUri != request.RedirectUri)
            throw new InvalidOperationException("Redirect URI mismatch.");
        
        if (grant.Pkce is not null && !grant.Pkce.Matches(request.CodeVerifier))
            throw new InvalidOperationException("Invalid PKCE code verifier.");
        
        
        return new TokenContext(
            grant.ClientId,
            grant.ClientId.ToString(),
            grant.Audience,
            grant.Scopes
        );
    }
}