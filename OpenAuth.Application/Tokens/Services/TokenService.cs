using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Application.Tokens.Interfaces;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Services;

public class TokenService : ITokenService
{
    private readonly Dictionary<GrantType, ITokenIssuer> _strategies;
    private readonly IClientQueryService _clientQueryService;
    private readonly ISigningKeyQueryService _signingKeyQueryService;
    private readonly IJwtTokenGenerator _tokenGenerator;
    
    public TokenService(IEnumerable<ITokenIssuer> strategies, IClientQueryService clientQueryService, ISigningKeyQueryService signingKeyQueryService, IJwtTokenGenerator tokenGenerator)
    {
        _strategies = strategies.ToDictionary(i => i.GrantType);
        _clientQueryService = clientQueryService;
        _signingKeyQueryService = signingKeyQueryService;
        _tokenGenerator = tokenGenerator;
        
        if (_strategies.Count == 0)
            throw new ArgumentException("No token issuer strategies registered.", nameof(strategies));
    }
    
    
    public async Task<TokenGenerationResponse> IssueToken(TokenRequest request, CancellationToken ct = default)
    {
        if (!_strategies.TryGetValue(request.GrantType, out var issuer))
            throw new InvalidOperationException("Invalid grant type.");
        
        var tokenData = await _clientQueryService.GetTokenDataAsync(request.ClientId, request.RequestedAudience, ct);
        if (tokenData is null)
            throw new InvalidOperationException("Client not found or does not have access to the requested audience.");
        
        if (request.RequestedScopes.Except(tokenData.AllowedScopes).Any())
            throw new InvalidOperationException($"Invalid scopes requested for audience '{request.RequestedAudience.Value}'.");


        var tokenContext = await issuer.IssueToken(request, ct);
    
        var keyData = await _signingKeyQueryService.GetCurrentKeyDataAsync(ct);
        if (keyData is null)
            throw new InvalidOperationException("No active signing key found.");
        
        var accessToken = _tokenGenerator.GenerateToken(tokenContext, tokenData, keyData);
        return new TokenGenerationResponse(accessToken, "Bearer", (int)tokenData.TokenLifetime.TotalSeconds);
    }
}