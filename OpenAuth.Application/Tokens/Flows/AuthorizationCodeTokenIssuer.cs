using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
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
            throw new InvalidOperationException("Client credentials are required for this grant type.");
        
        var grant = await _grantStore.GetAsync(command.Code)
                    ?? throw new InvalidOperationException("Invalid authorization code.");
        
        if (grant.Consumed)
            throw new InvalidOperationException("Authorization code has already been used.");

        if (grant.ClientId != command.ClientId)
            throw new InvalidOperationException("Client ID mismatch.");
        
        if (grant.RedirectUri != command.RedirectUri)
            throw new InvalidOperationException("Redirect URI mismatch.");

        if (grant.Scopes != command.RequestedScopes)
            throw new InvalidOperationException("Scopes mismatch.");
        
        // TODO: add better suited query
        var client = await _clientQueryService.GetTokenDataAsync(command.ClientId, ct)
            ?? throw new InvalidOperationException("Client not found.");
        
        var audience = client.AllowedAudiences.FirstOrDefault(a => a.Name == command.RequestedAudience);
        if (audience is null)
            throw new InvalidOperationException("Invalid audience.");
        
        if (!grant.Scopes.All(s => audience.Scopes.Contains(s)))
            throw new InvalidOperationException("Requested scopes not allowed for this audience.");
        

        if (grant.Pkce is not null)
        {
            if (!grant.Pkce.Matches(command.CodeVerifier))
                throw new InvalidOperationException("Invalid PKCE code verifier.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(command.ClientSecret))
                throw new InvalidOperationException("Client secret is required for this grant type.");
            
            if (!await _secretQueryService.ValidateSecretAsync(command.ClientId, command.ClientSecret, ct))
                throw new InvalidOperationException("Invalid client credentials.");
        }

        await _grantStore.RemoveAsync(grant.Code);
        
        return new TokenContext(
            grant.ClientId,
            grant.ClientId.ToString(),
            command.RequestedAudience,
            grant.Scopes
        );
    }
}