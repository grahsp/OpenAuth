using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Queries.GetClientApiScopes;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Extensions;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public class AuthorizationRequestValidator : IAuthorizationRequestValidator
{
    private readonly IQueryHandler<GetClientApiScopesQuery, ScopeCollection> _scopesHandler;
    private readonly IClientQueryService _clientQueryService;
    
    public AuthorizationRequestValidator(IQueryHandler<GetClientApiScopesQuery, ScopeCollection> scopesHandler, IClientQueryService clientQueryService)
    {
        _scopesHandler = scopesHandler;
        _clientQueryService = clientQueryService;
    }
    
    public async Task<AuthorizationValidationResult> ValidateAsync(AuthorizeCommand command, CancellationToken ct = default)
    {
        var authData =  await _clientQueryService.GetAuthorizationDataAsync(command.ClientId, ct)
                        ?? throw new InvalidClientException("Unknown client.");

        ValidateResponseType(command.ResponseType);
        var redirectUri = ValidateRedirectUri(command.RedirectUri, authData);

        await ValidateScopes(command.ClientId, command.Audience, command.Scopes, ct);
        ValidateOidc(command.Scopes);
        
        ValidateNonce(command.Scopes, command.Nonce, command.ResponseType);
        ValidatePkce(command.Pkce, authData);

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

    private static RedirectUri ValidateRedirectUri(RedirectUri requested, ClientAuthorizationData authData)
    {
        if (!authData.RedirectUris.Any(uri => Equals(requested, uri)))
            throw new InvalidRedirectUriException("Invalid redirect uri.");

        return requested;
    }

    private async Task ValidateScopes(ClientId clientId, AudienceIdentifier audience, ScopeCollection requested, CancellationToken ct)
    {
        if (requested.Count == 0)
            throw new InvalidScopeException("At least one scope is required.");

        var apiScopes = requested.GetApiScopes();

        if (apiScopes.IsEmpty)
            return;
        
        var query = new GetClientApiScopesQuery(clientId, audience);
        var allowedScopes = await _scopesHandler.HandleAsync(query, ct);
        
        if (!apiScopes.IsSubsetOf(allowedScopes))
            throw new InvalidScopeException("Request contained an invalid scope.");
    }

    private static void ValidatePkce(Pkce? requested, ClientAuthorizationData authData)
    {
        if (authData.IsClientPublic && requested is null)
            throw new InvalidRequestException("PKCE is required for public client.");
    }

    private static void ValidateOidc(ScopeCollection scopes)
    {
        if (scopes.ContainsOidcScopes() && !scopes.ContainsOpenIdScope())
            throw new InvalidRequestException("'openid' scope must be included for OIDC scopes.");
    }

    private static void ValidateNonce(ScopeCollection scopes, string? nonce, string responseType)
    {
        if (!scopes.ContainsOidcScopes())
            return;
        
        if (responseType != "code" && string.IsNullOrWhiteSpace(nonce))
            throw new InvalidRequestException("Nonce is required for OIDC scopes.");
    }
}