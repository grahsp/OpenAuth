using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Extensions;
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

    public async Task<AuthorizationGrant> AuthorizeAsync(AuthorizeCommand cmd, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        
        var authData = await _clientQueryService.GetAuthorizationDataAsync(cmd.ClientId, ct)
                       ?? throw new InvalidClientException("Unknown client.");
        
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
            cmd.Nonce,
            _time.GetUtcNow()
        );
        
        await _store.AddAsync(authorizationGrant);
        return authorizationGrant;
    }
    
    private static void ValidateRequest(
        ClientAuthorizationData authData,
        AuthorizeCommand cmd)
    {
        if (!authData.RedirectUris.Contains(cmd.RedirectUri))
            throw new InvalidRedirectUriException("Invalid redirect uri.");
        
        if (cmd.ResponseType != "code")
            throw new UnsupportedResponseTypeException($"'{cmd.ResponseType}' is not a supported response type.");
        
        if (cmd.Scopes.Count == 0)
            throw new InvalidScopeException("At least one scope is required.");
        
        if (authData.IsClientPublic && cmd.Pkce is null)
            throw new InvalidRequestException("PKCE is required for public client.");

        if (authData.IsClientPublic && cmd.Scopes.ContainsOpenIdScope() && string.IsNullOrWhiteSpace(cmd.Nonce))
            throw new InvalidRequestException("Nonce is required in public OIDC flow.");
    }
    
    private static string GenerateCode()
        => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
}