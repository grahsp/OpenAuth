using OpenAuth.Application.Dtos;
using OpenAuth.Application.Queries;

namespace OpenAuth.Application.Security.Tokens;

public class TokenService : ITokenService
{
    private readonly IClientQueryService _clientQueryService;
    private readonly ISecretQueryService _secretQueryService;
    private readonly ISigningKeyQueryService _signingKeyQueryService;
    private readonly IJwtTokenGenerator _tokenGenerator;
    
    public TokenService(IClientQueryService clientQueryService, ISecretQueryService secretQueryService, ISigningKeyQueryService signingKeyQueryService, IJwtTokenGenerator tokenGenerator)
    {
        _clientQueryService = clientQueryService;
        _secretQueryService = secretQueryService;
        _signingKeyQueryService = signingKeyQueryService;
        _tokenGenerator = tokenGenerator;
    }
    
    
    public async Task<TokenGenerationResponse> IssueToken(IssueTokenRequest request, CancellationToken ct = default)
    {
        var isValid = await _secretQueryService.ValidateSecretAsync(request.ClientId, request.ClientSecret, ct);
        if (!isValid)
            throw new UnauthorizedAccessException("Invalid client credentials.");
        
        var tokenData = await _clientQueryService.GetTokenDataAsync(request.ClientId, request.AudienceName, ct);
        if (tokenData is null)
            throw new InvalidOperationException("Client not found or does not have access to the requested audience.");

        if (request.RequestedScopes.Except(tokenData.AllowedScopes).Any())
            throw new InvalidOperationException($"Invalid scopes requested for audience '{request.AudienceName.Value}'.");
    
        var keyData = await _signingKeyQueryService.GetCurrentKeyDataAsync(ct);
        if (keyData is null)
            throw new InvalidOperationException("No active signing key found.");

        var tokenRequest = new TokenGenerationRequest(
            request.ClientId,
            request.AudienceName,
            request.RequestedScopes,
            tokenData.TokenLifetime,
            keyData
        );
        
        var accessToken = _tokenGenerator.GenerateToken(tokenRequest);
        return new TokenGenerationResponse(accessToken, "Bearer", (int)tokenData.TokenLifetime.TotalSeconds);
    }
}