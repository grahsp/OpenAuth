using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Application.Tokens.Interfaces;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Services;

public class TokenService : ITokenService
{
    private readonly Dictionary<GrantType, ITokenRequestProcessor> _strategies;
    private readonly IClientQueryService _clientQueryService;
    private readonly ISigningKeyQueryService _signingKeyQueryService;
    private readonly IJwtTokenGenerator _tokenGenerator;
    
    public TokenService(IEnumerable<ITokenRequestProcessor> strategies, IClientQueryService clientQueryService, ISigningKeyQueryService signingKeyQueryService, IJwtTokenGenerator tokenGenerator)
    {
        _strategies = strategies.ToDictionary(i => i.GrantType);
        _clientQueryService = clientQueryService;
        _signingKeyQueryService = signingKeyQueryService;
        _tokenGenerator = tokenGenerator;
        
        if (_strategies.Count == 0)
            throw new ArgumentException("No token issuer strategies registered.", nameof(strategies));
    }
    
    
    public async Task<TokenGenerationResponse> IssueToken(TokenCommand command, CancellationToken ct = default)
    {
        if (!_strategies.TryGetValue(command.GrantType, out var issuer))
            throw new InvalidRequestException("Invalid grant type.");
        
        var tokenData = await _clientQueryService.GetTokenDataAsync(command.ClientId, ct) ??
                        throw new InvalidClientException("Invalid client id.");
        
        if (!tokenData.AllowedGrantTypes.Contains(command.GrantType))
            throw new UnauthorizedClientException("grant_type not allowed on client.");
        
        // TODO: only include in flows that depend on audience and scope like client credentials.
        if (command.RequestedAudience is not null)
        {
            var audience = tokenData.AllowedAudiences
                .FirstOrDefault(a => a.Name == command.RequestedAudience);
            if (audience is null)
                throw new InvalidScopeException("Invalid audience.");
        
            if (command.RequestedScopes is not null)
            {
                var invalidScopes = command.RequestedScopes
                    .Except(audience.Scopes)
                    .Select(s => s.Value)
                    .ToArray();
                
                if (invalidScopes.Length > 0)
                    throw new InvalidScopeException("Invalid scope.");
            }
        }

        var tokenContext = await issuer.IssueToken(command, ct);
    
        var keyData = await _signingKeyQueryService.GetCurrentKeyDataAsync(ct);
        if (keyData is null)
            throw new ServerErrorException("No active signing key available.");
        
        var accessToken = _tokenGenerator.GenerateToken(tokenContext, tokenData, keyData);
        return new TokenGenerationResponse(accessToken, "Bearer", (int)tokenData.TokenLifetime.TotalSeconds);
    }
}