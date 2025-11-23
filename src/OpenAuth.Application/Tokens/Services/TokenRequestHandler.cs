using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Services;

public class TokenRequestHandler : ITokenRequestHandler
{
    private readonly Dictionary<GrantType, ITokenRequestProcessor> _strategies;
    private readonly IClientQueryService _clientQueryService;

    private readonly ITokenHandler<AccessTokenContext> _accessTokens;
    
    public TokenRequestHandler(IEnumerable<ITokenRequestProcessor> strategies, IClientQueryService clientQueryService, ITokenHandler<AccessTokenContext> accessTokens)
    {
        _strategies = strategies.ToDictionary(i => i.GrantType);
        _clientQueryService = clientQueryService;
        _accessTokens = accessTokens;
        
        if (_strategies.Count == 0)
            throw new ArgumentException("No token issuer strategies registered.", nameof(strategies));
    }
    
    
    public async Task<TokenGenerationResponse> IssueToken(TokenCommand command, CancellationToken ct = default)
    {
        if (!_strategies.TryGetValue(command.GrantType, out var processor))
            throw new InvalidRequestException("Invalid grant type.");
        
        var tokenData = await _clientQueryService.GetTokenDataAsync(command.ClientId, ct) ??
                        throw new InvalidClientException("Invalid client id.");
        
        if (!tokenData.AllowedGrantTypes.Contains(command.GrantType))
            throw new UnauthorizedClientException("grant_type not allowed on client.");

        var tokenContext = await processor.ProcessAsync(command, tokenData, ct);

        var accessTokenContext = new AccessTokenContext(tokenData, tokenContext.Subject, command.RequestedScopes,
            command.RequestedAudience);
        
        var accessToken = await _accessTokens.CreateAsync(accessTokenContext, ct);
        return new TokenGenerationResponse(accessToken, "Bearer", (int)tokenData.TokenLifetime.TotalSeconds);
    }
}