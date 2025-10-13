using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

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
        throw new NotImplementedException();
    }
}