using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;
using TokenContext = OpenAuth.Application.Tokens.Dtos.TokenContext;

namespace OpenAuth.Application.Tokens.Flows;

public class AuthorizationCodeTokenIssuer : TokenIssuerBase<AuthorizationCodeTokenRequest>
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
    
    protected override async Task<TokenContext> IssueToken(AuthorizationCodeTokenRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.CodeVerifier) && string.IsNullOrWhiteSpace(request.ClientSecret))
            throw new InvalidOperationException("Client credentials are required for this grant type.");
        
        var grant = await _grantStore.GetAsync(request.Code)
                    ?? throw new InvalidOperationException("Invalid authorization code.");
        
        if (grant.Consumed)
            throw new InvalidOperationException("Authorization code has already been used.");

        if (grant.ClientId != request.ClientId)
            throw new InvalidOperationException("Client ID mismatch.");
        
        if (grant.Subject != request.Subject)
            throw new InvalidOperationException("Subject mismatch.");
        
        if (grant.RedirectUri != request.RedirectUri)
            throw new InvalidOperationException("Redirect URI mismatch.");

        if (grant.Scopes != request.RequestedScopes)
            throw new InvalidOperationException("Scopes mismatch.");
        
        // TODO: add better suited query
        var client = await _clientQueryService.GetTokenDataAsync(request.ClientId, ct)
            ?? throw new InvalidOperationException("Client not found.");
        
        var audience = client.AllowedAudiences.FirstOrDefault(a => a.Name == request.RequestedAudience);
        if (audience is null)
            throw new InvalidOperationException("Invalid audience.");
        
        if (!grant.Scopes.All(s => audience.Scopes.Contains(s)))
            throw new InvalidOperationException("Requested scopes not allowed for this audience.");
        

        if (grant.Pkce is not null)
        {
            if (!grant.Pkce.Matches(request.CodeVerifier))
                throw new InvalidOperationException("Invalid PKCE code verifier.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.ClientSecret))
                throw new InvalidOperationException("Client secret is required for this grant type.");
            
            if (!await _secretQueryService.ValidateSecretAsync(request.ClientId, request.ClientSecret, ct))
                throw new InvalidOperationException("Invalid client credentials.");
        }

        await _grantStore.RemoveAsync(grant.Code);
        
        return new TokenContext(
            grant.ClientId,
            grant.ClientId.ToString(),
            request.RequestedAudience,
            grant.Scopes
        );
    }
}