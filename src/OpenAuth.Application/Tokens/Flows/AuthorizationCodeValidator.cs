using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Extensions;
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

    public async Task<AuthorizationCodeValidationResult> ValidateAsync(AuthorizationCodeValidatorContext context, CancellationToken ct = default)
    {
        var command = context.Command;
        var tokenData = context.TokenData;
        var authorizationGrant = context.AuthorizationGrant;
        
        ValidateUnusedAuthorizationGrant(authorizationGrant);
        ValidateAuthorizationGrantBinding(command, authorizationGrant);
        await ValidateClientAuthenticationAsync(command, authorizationGrant, ct);
        
        var apiScopes = authorizationGrant.GrantedScopes.GetApiScopes();
        var oidcScopes = authorizationGrant.GrantedScopes.GetOidcScopes();

        var audience = apiScopes.Count > 0
            ? tokenData.AllowedAudiences.FirstOrDefault(a => apiScopes.IsSubsetOf(a.Scopes))
            : null;

        return new AuthorizationCodeValidationResult(audience?.Name, authorizationGrant.GrantedScopes, oidcScopes);
    }

    private static void ValidateUnusedAuthorizationGrant(AuthorizationGrant authorizationGrant)
    {
        if (authorizationGrant.Consumed)
            throw new InvalidGrantException("Authorization code has already been redeemed.");
    }
    
    private async Task ValidateClientAuthenticationAsync(AuthorizationCodeTokenCommand command, AuthorizationGrant authorizationGrant, CancellationToken ct)
    {
        if (authorizationGrant.Pkce is not null)
            ValidatePkce(command, authorizationGrant);
        else
            await ValidateClientSecretAsync(command, ct);
    }

    private static void ValidateAuthorizationGrantBinding(AuthorizationCodeTokenCommand command,
        AuthorizationGrant authorizationGrant)
    {
        if (authorizationGrant.ClientId != command.ClientId)
            throw new InvalidGrantException("Authorization code was issued to another client.");

        if (authorizationGrant.RedirectUri != command.RedirectUri)
            throw new InvalidGrantException("'redirect_uri' does not match authorization request.");
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