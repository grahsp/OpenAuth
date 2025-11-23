using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public class AuthorizationCodeGrantProcessor : TokenRequestProcessor<AuthorizationCodeTokenCommand>
{
    public override GrantType GrantType => GrantType.AuthorizationCode;

    private readonly IAuthorizationGrantStore _grantStore;
    private readonly IAuthorizationCodeValidator _validator;
    
    public AuthorizationCodeGrantProcessor(IAuthorizationGrantStore grantStore, IAuthorizationCodeValidator validator)
    {
        _grantStore = grantStore;
        _validator = validator;
    }
    
    protected override async Task<TokenContext> ProcessAsync(AuthorizationCodeTokenCommand command, CancellationToken ct = default)
    {
        var authorizationGrant = await _grantStore.GetAsync(command.Code)
                    ?? throw new InvalidGrantException("Invalid or unknown authorization code.");

        var result = await _validator.ValidateAsync(command, authorizationGrant, ct);

        authorizationGrant.Consume();
        await _grantStore.RemoveAsync(authorizationGrant.Code);
        
        return new TokenContext(
            result.AuthorizationGrant.ClientId,
            result.AuthorizationGrant.Subject,
            result.Audience.Name,
            result.AuthorizationGrant.GrantedScopes
        );
    }
}