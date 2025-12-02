using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Extensions;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public class AuthorizationRequestValidator : IAuthorizationRequestValidator
{
    private readonly IClientQueryService _clientQueryService;
    
    public AuthorizationRequestValidator(IClientQueryService clientQueryService)
    {
        _clientQueryService = clientQueryService;
    }
    
    public async Task<AuthorizationValidationResult> ValidateAsync(AuthorizeCommand command, CancellationToken ct)
    {
        var authData =  await _clientQueryService.GetAuthorizationDataAsync(command.ClientId, ct)
                        ?? throw new InvalidClientException("Unknown client.");

        ValidateResponseType(command.ResponseType);
        var redirectUri = ValidateRedirectUri(command.RedirectUri, authData);
        ValidateScopes(command.Scopes, authData);
        
        ValidatePkce(command.Pkce, authData);
        ValidateOidc(command, authData);


        return new AuthorizationValidationResult(
            command.ClientId,
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

    private static void ValidateScopes(ScopeCollection requested, ClientAuthorizationData authData)
    {
        if (requested.Count == 0)
            throw new InvalidScopeException("At least one scope is required.");
        
        // TODO: validate against clients allowed scopes
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