using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.Oidc;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;
using TokenContext = OpenAuth.Application.Tokens.Dtos.TokenContext;

namespace OpenAuth.Application.Tokens.Flows;

public class AuthorizationCodeTokenIssuer : TokenIssuerBase<AuthorizationCodeTokenCommand>
{
    private readonly IAuthorizationGrantStore _grantStore;
    private readonly IClientQueryService _clientQueryService;
    private readonly ISecretQueryService _secretQueryService;
    
    public override GrantType GrantType => GrantType.AuthorizationCode;

    public AuthorizationCodeTokenIssuer(IAuthorizationGrantStore grantStore, IClientQueryService clientQueryService, ISecretQueryService secretQueryService)
    {
        _grantStore = grantStore;
        _clientQueryService = clientQueryService;
        _secretQueryService = secretQueryService;
    }
    
    protected override async Task<TokenContext> IssueToken(AuthorizationCodeTokenCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.CodeVerifier) && string.IsNullOrWhiteSpace(command.ClientSecret))
            throw new InvalidRequestException("Either 'code_verifier' or 'client_secret' must be provided.");
        
        var grant = await _grantStore.GetAsync(command.Code)
                    ?? throw new InvalidGrantException("Invalid or unknown authorization code.");
        
        if (grant.Consumed)
            throw new InvalidGrantException("Authorization code has already been redeemed.");

        if (grant.ClientId != command.ClientId)
            throw new InvalidGrantException("Authorization code was issued to another client.");
        
        if (grant.RedirectUri != command.RedirectUri)
            throw new InvalidGrantException("'redirect_uri' does not match authorization request.");

        // TODO: remove as authorization code does not need to send scope.
        if (grant.GrantedScopes != command.RequestedScopes)
            throw new InvalidGrantException("'scope' does not match authorization request.");
        
        // TODO: add better suited query
        var client = await _clientQueryService.GetTokenDataAsync(command.ClientId, ct)
            ?? throw new InvalidClientException("Client is not registered.");
        
        var audience = client.AllowedAudiences.FirstOrDefault(a => a.Name == command.RequestedAudience)
            ?? throw new InvalidScopeException("Requested audience is not allowed.");
        
        if (!grant.GrantedScopes.All(s => audience.Scopes.Contains(s)))
            throw new InvalidScopeException("One or more scopes are not allowed.");
        

        if (grant.Pkce is not null)
        {
            if (!grant.Pkce.Matches(command.CodeVerifier))
                throw new InvalidGrantException("Invalid PKCE code verifier.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(command.ClientSecret))
                throw new InvalidClientException("Client secret is required.");
            
            if (!await _secretQueryService.ValidateSecretAsync(command.ClientId, command.ClientSecret, ct))
                throw new InvalidClientException("Invalid client credentials.");
        }

        await _grantStore.RemoveAsync(grant.Code);
        
        return new TokenContext(
            grant.ClientId,
            grant.ClientId.ToString(),
            command.RequestedAudience,
            grant.GrantedScopes
        );
    }
}