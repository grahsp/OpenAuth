using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.OAuth.Authorization.Dtos;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public class AuthorizationHandler : IAuthorizationHandler
{
    private readonly IClientQueryService _clientQueryService;
    private readonly IAuthorizationGrantStore _store;
    private readonly TimeProvider _time;
    
    public AuthorizationHandler(
        IClientQueryService clientQueryService,
        IAuthorizationGrantStore store,
        TimeProvider time)
    {
        _clientQueryService = clientQueryService;
        _store = store;
        _time = time;
    }

    public async Task<AuthorizationResponse> AuthorizeAsync(AuthorizationRequest request, string subject)
    {
        var authorizationData = await _clientQueryService.GetAuthorizationDataAsync(request.ClientId)
                                ?? throw new InvalidOperationException("Client not found.");

        ValidateRequest(authorizationData, request);

        var code = GenerateCode();
        var authorizationGrant = AuthorizationGrant.Create(
            code,
            GrantType.AuthorizationCode,
            subject,
            request.ClientId,
            request.RedirectUri,
            request.Audience,
            request.Scopes,
            _time.GetUtcNow(),
            request.Pkce
        );
        
        await _store.AddAsync(authorizationGrant);
        
        return new AuthorizationResponse(authorizationGrant.Code, authorizationGrant.RedirectUri);
    }

    private static void ValidateRequest(ClientAuthorizationData authorizationData, AuthorizationRequest request)
    {
        if (authorizationData.IsClientPublic && request.Pkce is null)
            throw new InvalidOperationException("Pkce is required.");

        if (!authorizationData.RedirectUris.Contains(request.RedirectUri))
            throw new InvalidOperationException("Invalid redirect URI.");
        
        var audience = authorizationData.AllowedAudiences
            .FirstOrDefault(a => a.Name == request.Audience);
        if (audience is null)
            throw new InvalidOperationException("Invalid audience.");
        
        var invalidScopes = request.Scopes.Except(audience.Scopes)
            .Select(s => s.Value)
            .ToArray();
        if (invalidScopes.Length != 0)
            throw new InvalidOperationException($"Invalid scopes: '{ string.Join(' ', invalidScopes) }'.");
    }

    private string GenerateCode()
        => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
}