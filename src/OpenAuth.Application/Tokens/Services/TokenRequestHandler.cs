using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Oidc;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Services;

public class TokenRequestHandler : ITokenRequestHandler
{
    private readonly Dictionary<GrantType, ITokenRequestProcessor> _strategies;
    private readonly IClientQueryService _clientQueryService;
    private readonly ITokenHandler<AccessTokenContext> _accessTokenHandler;
    private readonly ITokenHandler<IdTokenContext> _idTokenHandler;
    
    public TokenRequestHandler(IEnumerable<ITokenRequestProcessor> strategies,
        IClientQueryService clientQueryService,
        ITokenHandler<AccessTokenContext> accessTokenHandler,
        ITokenHandler<IdTokenContext> idTokenHandler)
    {
        var strats = strategies.ToArray();
        if (strats.Length == 0)
            throw new ArgumentException("No token issuer strategies registered.", nameof(strategies));

        _strategies = strats.ToDictionary(i => i.GrantType);
        _clientQueryService = clientQueryService;
        _accessTokenHandler = accessTokenHandler;
        _idTokenHandler = idTokenHandler;
    }
    
    
    public async Task<TokenResult> HandleAsync(TokenCommand command, CancellationToken ct = default)
    {
        if (!_strategies.TryGetValue(command.GrantType, out var processor))
            throw new InvalidRequestException("Invalid grant type.");
        
        var tokenData = await _clientQueryService.GetTokenDataAsync(command.ClientId, ct) ??
                        throw new InvalidClientException("Invalid client id.");
        
        if (!tokenData.AllowedGrantTypes.Contains(command.GrantType))
            throw new UnauthorizedClientException("grant_type not allowed on client.");
        
        var tokenContext = await processor.ProcessAsync(command, tokenData, ct);

        var accessToken = await CreateAccessTokenAsync(tokenContext, tokenData, ct);
        var idToken = await CreateIdTokenAsync(tokenContext, tokenData, ct);

        return new TokenResult(accessToken, "Bearer", (int)tokenData.TokenLifetime.TotalSeconds, idToken);
    }

    private async Task<string?> CreateAccessTokenAsync(TokenContext tokenContext, ClientTokenData tokenData, CancellationToken ct)
    {
        if (tokenContext.Audience is null)
            return null;
        
        var accessTokenContext = new AccessTokenContext(
            tokenContext.ClientId,
            tokenContext.Audience,
            tokenContext.Subject,
            (int)tokenData.TokenLifetime.TotalSeconds,
            tokenContext.Scopes
        );
        
        return await _accessTokenHandler.CreateAsync(accessTokenContext, ct);
    }

    private async Task<string?> CreateIdTokenAsync(TokenContext tokenContext, ClientTokenData tokenData, CancellationToken ct)
    {
        var oidcContext = tokenContext.OidcContext;
        if (oidcContext is null)
            return null;

        if (tokenContext.ClientId is null)
            throw new InvalidOperationException("Expected client id to be present.");
        
        if (tokenContext.Subject is null)
            throw new InvalidOperationException("Expected subject to be present.");
        
        var idTokenContext = new IdTokenContext(
            tokenContext.ClientId,
            tokenContext.Subject,
            oidcContext.Nonce,
            oidcContext.AuthTimeInSeconds,
            (int)tokenData.TokenLifetime.TotalSeconds,
            oidcContext.Scopes
        );
        
        return await _idTokenHandler.CreateAsync(idTokenContext, ct);
    }
}