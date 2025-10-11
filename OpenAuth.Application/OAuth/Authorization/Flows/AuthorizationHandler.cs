using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;

namespace OpenAuth.Application.OAuth.Authorization.Flows;

public class AuthorizationHandler : IAuthorizationHandler
{
    private readonly IClientQueryService _clientQueryService;
    private readonly TimeProvider _time;
    
    public AuthorizationHandler(IClientQueryService clientQueryService, TimeProvider time)
    {
        _clientQueryService = clientQueryService;
        _time = time;
    }
    
    public async Task<AuthorizationResponse> AuthorizeAsync(AuthorizationRequest request)
    {
        var authorizationData = await _clientQueryService.GetAuthorizationDataAsync(request.ClientId)
                                ?? throw new InvalidOperationException("Client not found.");
        
        ValidateRequest(authorizationData, request);

        var code = GenerateCode();
        return new AuthorizationResponse(code, request.RedirectUri);
    }

    private void ValidateRequest(ClientAuthorizationData authorizationData, AuthorizationRequest request)
    {
        if (!authorizationData.GrantTypes.Contains(request.GrantType))
            throw new InvalidOperationException("Invalid grant type.");
        

        if (authorizationData.RequirePkce)
        {
            if (string.IsNullOrWhiteSpace(request.CodeChallenge))
                throw new InvalidOperationException("Code challenge is required for PKCE.");
            
            if (string.IsNullOrWhiteSpace(request.CodeChallengeMethod))
                throw new InvalidOperationException("Code challenge method is required for PKCE.");
        }

        if (!authorizationData.RedirectUris.Contains(request.RedirectUri))
            throw new InvalidOperationException("Invalid redirect URI.");
        
        var audience = authorizationData.AllowedAudiences
            .FirstOrDefault(a => a.Name == request.Audience);
        if (audience is null)
            throw new InvalidOperationException("Invalid audience.");
        
        if (!request.Scopes.All(s => audience.AllowedScopes.Contains(s)))
            throw new InvalidOperationException("Invalid scopes.");
    }

    private string GenerateCode()
        => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
}