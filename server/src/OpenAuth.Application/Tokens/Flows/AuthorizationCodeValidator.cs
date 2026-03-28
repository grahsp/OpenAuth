using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Extensions;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.ApiResources;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public class AuthorizationCodeValidator : IAuthorizationCodeValidator
{
    private readonly ISecretQueryService _secretQueryService;
    
    public AuthorizationCodeValidator(ISecretQueryService secretQueryService)
    {
        _secretQueryService = secretQueryService;
    }

    public async Task<AuthorizationCodeValidationResult> ValidateAsync(
        AuthorizationCodeTokenCommand command,
        ClientTokenData tokenData,
        AuthorizationGrant authorizationGrant,
        ApiResource api,
        CancellationToken ct = default)
    {
        ValidateUnusedAuthorizationGrant(authorizationGrant);
        ValidateAuthorizationGrantBinding(command, authorizationGrant);
        await ValidateClientAuthenticationAsync(command, authorizationGrant, ct);
        
        ValidateAudience(authorizationGrant, api);
        var scope = ValidateScopes(command.RequestedScopes, authorizationGrant.GrantedScopes);
        
        return new AuthorizationCodeValidationResult(scope.GetApiScopes(), scope.GetOidcScopes());
    }
    
    private static void ValidateAudience(AuthorizationGrant grant, ApiResource api)
    {
        if (grant.Audience != api.Audience)
            throw new InvalidGrantException("Audience mismatch.");
    }
    
    private static ScopeCollection ValidateScopes(ScopeCollection? requested, ScopeCollection granted)
    {
        if (requested is null)
            return granted;
        
        if (!requested.GetApiScopes().IsSubsetOf(granted))
            throw new InvalidScopeException("Request contained an invalid scope.");

        return requested;
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