using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Extensions;
using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public class AuthorizationRequestValidator : IAuthorizationRequestValidator
{
    private readonly IApiResourceRepository _apiResourceRepository;
    private readonly IClientQueryService _clientQueryService;
    
    public AuthorizationRequestValidator(IApiResourceRepository apiResourceRepository, IClientQueryService clientQueryService)
    {
        _apiResourceRepository = apiResourceRepository;
        _clientQueryService = clientQueryService;
    }
    
    public async Task<AuthorizationValidationResult> ValidateAsync(AuthorizeCommand command, CancellationToken ct)
    {
        var authData =  await _clientQueryService.GetAuthorizationDataAsync(command.ClientId, ct)
                        ?? throw new InvalidClientException("Unknown client.");

        ValidateResponseType(command.ResponseType);
        var redirectUri = ValidateRedirectUri(command.RedirectUri, authData);

        var apiScopes = await ValidateAudienceAsync(command.Audience);
        ValidateScopes(command.Scopes, apiScopes);
        
        ValidatePkce(command.Pkce, authData);
        ValidateOidc(command, authData);

        return new AuthorizationValidationResult(
            command.ClientId,
            command.Audience,
            command.Scopes,
            redirectUri,
            command.Pkce,
            command.Nonce
        );
    }

    private static void ValidateResponseType(string responseType)
    {
        if (responseType != "code")
            throw new UnsupportedResponseTypeException($"'{responseType}' is not a supported response type.");
    }

    private static RedirectUri ValidateRedirectUri(RedirectUri? requested, ClientAuthorizationData authData)
    {
        if (requested is null && authData.RedirectUris.Length == 1)
            return authData.RedirectUris.First();
        
        if (requested is null)
            throw new InvalidRedirectUriException("Missing redirect uri.");
        
        if (!authData.RedirectUris.Any(uri => Equals(requested, uri)))
            throw new InvalidRedirectUriException("Invalid redirect uri.");

        return requested;
    }

    private async Task<ScopeCollection> ValidateAudienceAsync(AudienceIdentifier audience)
    {
        var api = await _apiResourceRepository.GetByAudienceAsync(audience)
            ?? throw new InvalidOperationException("API not found.");

        var scopes = api.Permissions
            .Select(p => p.Scope)
            .ToArray();
        
        return new ScopeCollection(scopes);
    }

    private static void ValidateScopes(ScopeCollection requested, ScopeCollection allowed)
    {
        if (requested.Count == 0)
            throw new InvalidScopeException("At least one scope is required.");
        
        if (!requested.IsSubsetOf(allowed))
            throw new InvalidScopeException("Request contained an invalid scope.");
    }

    private static void ValidatePkce(Pkce? requested, ClientAuthorizationData authData)
    {
        if (authData.IsClientPublic && requested is null)
            throw new InvalidRequestException("PKCE is required for public client.");
    }

    private static void ValidateOidc(AuthorizeCommand command, ClientAuthorizationData authData)
    {
        if (command.Scopes.ContainsOidcScopes() && !command.Scopes.ContainsOpenIdScope())
            throw new InvalidRequestException("'openid' scope must be included for OIDC scopes.");
        
        if (command.ResponseType != "code" && command.Scopes.ContainsOpenIdScope() && string.IsNullOrWhiteSpace(command.Nonce))
            throw new InvalidRequestException("Nonce is required for OIDC scopes.");
    }
}