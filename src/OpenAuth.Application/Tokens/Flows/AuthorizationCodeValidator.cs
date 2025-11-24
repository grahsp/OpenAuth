using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.AuthorizationGrants;

namespace OpenAuth.Application.Tokens.Flows;

public class AuthorizationCodeValidator : IAuthorizationCodeValidator
{
    private readonly ISecretQueryService _secretQueryService;
    
    public AuthorizationCodeValidator(ISecretQueryService secretQueryService)
    {
        _secretQueryService = secretQueryService;
    }

    public async Task<AuthorizationCodeValidationResult> ValidateAsync(AuthorizationCodeTokenCommand command, ClientTokenData tokenData, AuthorizationGrant authorizationGrant, CancellationToken ct = default)
    {
        if (authorizationGrant.Consumed)
            throw new InvalidGrantException("Authorization code has already been redeemed.");

        if (authorizationGrant.ClientId != command.ClientId)
            throw new InvalidGrantException("Authorization code was issued to another client.");
        
        if (authorizationGrant.RedirectUri != command.RedirectUri)
            throw new InvalidGrantException("'redirect_uri' does not match authorization request.");

        // TODO: remove as authorization code does not need to send scope.
        if (authorizationGrant.GrantedScopes != command.RequestedScopes)
            throw new InvalidGrantException("'scope' does not match authorization request.");
        
        var audience = tokenData.AllowedAudiences.FirstOrDefault(a => a.Name == command.RequestedAudience)
                       ?? throw new InvalidScopeException("Requested audience is not allowed.");
        
        if (!authorizationGrant.GrantedScopes.All(s => audience.Scopes.Contains(s)))
            throw new InvalidScopeException("One or more scopes are not allowed.");
        
        await ValidateClientAuthenticationAsync(command, authorizationGrant, ct);

        return new AuthorizationCodeValidationResult(authorizationGrant, tokenData, audience);
    }


    private async Task ValidateClientAuthenticationAsync(AuthorizationCodeTokenCommand command, AuthorizationGrant authorizationGrant, CancellationToken ct)
    {
        if (authorizationGrant.Pkce is not null)
            ValidatePkce(command, authorizationGrant);
        else
            await ValidateClientSecretAsync(command, ct);
    }

    private static void ValidatePkce(AuthorizationCodeTokenCommand command, AuthorizationGrant authorizationGrant)
    {
        if (string.IsNullOrWhiteSpace(command.CodeVerifier))
            throw new InvalidGrantException("Missing 'code_verifier'.");
        
        if (!authorizationGrant.Pkce!.Matches(command.CodeVerifier))
            throw new InvalidGrantException("Invalid PKCE code verifier.");
    }

    private async Task ValidateClientSecretAsync(AuthorizationCodeTokenCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.ClientSecret))
            throw new InvalidClientException("Missing client secret.");

        if (!await _secretQueryService.ValidateSecretAsync(command.ClientId, command.ClientSecret, ct))
            throw new InvalidClientException("Invalid client credentials.");
    }
}