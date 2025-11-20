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
        ArgumentNullException.ThrowIfNull(cmd);
        
        var authData = await _clientQueryService.GetAuthorizationDataAsync(cmd.ClientId, ct)
                       ?? throw new InvalidOperationException("Client not found.");
        
        ValidateRequest(authData, cmd);

        var code = GenerateCode();
        var authorizationGrant = AuthorizationGrant.Create(
            code,
            GrantType.AuthorizationCode,
            cmd.Subject,
            cmd.ClientId,
            cmd.RedirectUri,
            cmd.Scopes,
            cmd.Pkce,
            _time.GetUtcNow()
        );
        
        await _store.AddAsync(authorizationGrant);
        return authorizationGrant;
    }
    
    private static void ValidateRequest(
        ClientAuthorizationData authData,
        AuthorizeCommand cmd)
    {
        if (cmd.ResponseType != "code")
            throw new InvalidOperationException($"Response type '{cmd.ResponseType}' not supported.");
        
        if (!authData.RedirectUris.Contains(cmd.RedirectUri))
            throw new InvalidOperationException("Invalid redirect URI.");
        
        if (!cmd.Scopes.Any())
            throw new InvalidOperationException("Scope is required.");
        
        if (authData.IsClientPublic && cmd.Pkce is null)
            throw new InvalidOperationException("Pkce is required.");
    }
    
    private static string GenerateCode()
        => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
}