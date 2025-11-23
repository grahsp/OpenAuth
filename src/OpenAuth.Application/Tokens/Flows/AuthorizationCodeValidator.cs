using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.AuthorizationGrants;

namespace OpenAuth.Application.Tokens.Flows;

public class AuthorizationCodeValidator : IAuthorizationCodeValidator
{
    private readonly IClientQueryService _clientQueryService;
    private readonly ISecretQueryService _secretQueryService;
    
    public AuthorizationCodeValidator(IClientQueryService clientQueryService, ISecretQueryService secretQueryService)
    {
        _clientQueryService = clientQueryService;
        _secretQueryService = secretQueryService;
    }

    public async Task<AuthorizationCodeValidationResult> ValidateAsync(AuthorizationCodeTokenCommand command, AuthorizationGrant authorizationGrant, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.CodeVerifier) && string.IsNullOrWhiteSpace(command.ClientSecret))
            throw new InvalidRequestException("Either 'code_verifier' or 'client_secret' must be provided.");
        
        if (authorizationGrant.Consumed)
            throw new InvalidGrantException("Authorization code has already been redeemed.");

        if (authorizationGrant.ClientId != command.ClientId)
            throw new InvalidGrantException("Authorization code was issued to another client.");
        
        if (authorizationGrant.RedirectUri != command.RedirectUri)
            throw new InvalidGrantException("'redirect_uri' does not match authorization request.");

        // TODO: remove as authorization code does not need to send scope.
        if (authorizationGrant.GrantedScopes != command.RequestedScopes)
            throw new InvalidGrantException("'scope' does not match authorization request.");
        
        // TODO: add better suited query
        var client = await _clientQueryService.GetTokenDataAsync(command.ClientId, ct)
                     ?? throw new InvalidClientException("Client is not registered.");
        
        var audience = client.AllowedAudiences.FirstOrDefault(a => a.Name == command.RequestedAudience)
                       ?? throw new InvalidScopeException("Requested audience is not allowed.");
        
        if (!authorizationGrant.GrantedScopes.All(s => audience.Scopes.Contains(s)))
            throw new InvalidScopeException("One or more scopes are not allowed.");

        if (authorizationGrant.Pkce is not null)
        {
            if (!authorizationGrant.Pkce.Matches(command.CodeVerifier))
                throw new InvalidGrantException("Invalid PKCE code verifier.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(command.ClientSecret))
                throw new InvalidClientException("Client secret is required.");
            
            if (!await _secretQueryService.ValidateSecretAsync(command.ClientId, command.ClientSecret, ct))
                throw new InvalidClientException("Invalid client credentials.");
        }

        return new AuthorizationCodeValidationResult(authorizationGrant, client, audience);
    }
}