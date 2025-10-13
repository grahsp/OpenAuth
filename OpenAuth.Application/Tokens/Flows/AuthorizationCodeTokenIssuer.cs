using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public class AuthorizationCodeTokenIssuer : TokenIssuerBase<AuthorizationCodeTokenRequest>
{
    public override GrantType GrantType => GrantType.AuthorizationCode;
    
    protected override Task<TokenContext> IssueToken(AuthorizationCodeTokenRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}