using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Extensions;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
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

    public async Task<AuthorizationCodeValidationResult> ValidateAsync(AuthorizationCodeValidatorContext context, CancellationToken ct = default)
    {
        var command = context.Command;
        var tokenData = context.TokenData;
        var authorizationGrant = context.AuthorizationGrant;
        
        ValidateUnusedAuthorizationGrant(authorizationGrant);
        ValidateAuthorizationGrantBinding(command, authorizationGrant);
        await ValidateClientAuthenticationAsync(command, authorizationGrant, ct);
        
        var audience = ExtractAudience(command, tokenData);
        var (apiScopes, oidcScopes) = ExtractScopes(audience, authorizationGrant);
        
        ValidateOidc(authorizationGrant, oidcScopes);

        return new AuthorizationCodeValidationResult(audience.Name, apiScopes, oidcScopes);
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

    private static Audience ExtractAudience(AuthorizationCodeTokenCommand command, ClientTokenData tokenData)
    {
        if (command.RequestedAudience is null)
            throw new InvalidRequestException("Invalid audience."); 
        
        var audience = tokenData.AllowedAudiences.FirstOrDefault(a => a.Name == command.RequestedAudience)
                       ?? throw new InvalidScopeException("Invalid audience.");

        return audience;
    }

    // TODO: No scope validation required in token request flow - move to authorization flow
    private static (ScopeCollection ApiScopes, ScopeCollection OidcScopes) ExtractScopes(Audience audience, AuthorizationGrant authorizationGrant)
    {
        if (authorizationGrant.GrantedScopes.Count == 0)
            throw new InvalidScopeException("No valid scopes found.");
        
        var apiScopes = authorizationGrant.GrantedScopes
            .GetFilteredApiScopes(audience.Scopes);

        var oidcScopes = authorizationGrant.GrantedScopes
            .GetOidcScopes();
        
        return (apiScopes, oidcScopes);
    }

    private static void ValidateOidc(AuthorizationGrant authorizationGrant, ScopeCollection oidcScopes)
    {
        if (oidcScopes.Count == 0)
            return;

        // TODO: optionally implement whitelisting of oidc scopes in client
        if (!oidcScopes.ContainsOpenIdScope())
            throw new InvalidScopeException("OIDC scopes require 'openid' scope.");
        
        if (authorizationGrant.Nonce is null)
            throw new InvalidGrantException("Nonce is required in OIDC.");
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