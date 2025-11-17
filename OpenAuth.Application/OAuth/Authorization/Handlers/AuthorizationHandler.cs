using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
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

    public async Task<AuthorizationGrant> AuthorizeAsync(AuthorizeCommand cmd, CancellationToken ct = default)
    {
        var clientId = ClientId.Create(cmd.ClientId);
        var redirectUri = RedirectUri.Create(cmd.RedirectUri);
        
        AudienceName.TryCreate(cmd.Audience, out var audience);
        ScopeCollection.TryParse(cmd.Scope, out var scope);
        Pkce.TryCreate(cmd.CodeChallenge, cmd.CodeChallengeMethod, out var pkce);
        
        var authData = await _clientQueryService.GetAuthorizationDataAsync(clientId, ct)
                       ?? throw new InvalidOperationException("Client not found.");

        ValidateRequest(authData, redirectUri, audience, scope, pkce);

        var code = GenerateCode();
        var authorizationGrant = AuthorizationGrant.Create(
            code,
            GrantType.AuthorizationCode,
            cmd.Subject,
            clientId,
            redirectUri,
            audience,
            scope,
            pkce,
            _time.GetUtcNow()
        );
        
        await _store.AddAsync(authorizationGrant);
        return authorizationGrant;
    }
    
    private static void ValidateRequest(
        ClientAuthorizationData authData,
        RedirectUri redirectUri,
        AudienceName? requestedAudience,
        ScopeCollection? requestedScope,
        Pkce? pkce)
    {
        if (authData.IsClientPublic && pkce is null)
            throw new InvalidOperationException("Pkce is required.");

        if (!authData.RedirectUris.Contains(redirectUri))
            throw new InvalidOperationException("Invalid redirect URI.");

        if (requestedAudience is not null)
        {
            var validAudience = authData.AllowedAudiences
                .FirstOrDefault(a => a.Name == requestedAudience);
            if (validAudience is null)
                throw new InvalidOperationException("Invalid audience.");

        
            if (requestedScope is null)
                return;
            
            var invalidScopes = requestedScope.Except(validAudience.Scopes)
                .Select(s => s.Value)
                .ToArray();
            if (invalidScopes.Length != 0)
                throw new InvalidOperationException($"Invalid scopes: '{ string.Join(' ', invalidScopes) }'.");
        }
    }
    
    private static string GenerateCode()
        => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
}