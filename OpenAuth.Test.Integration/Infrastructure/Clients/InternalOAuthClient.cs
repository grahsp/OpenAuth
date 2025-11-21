using OpenAuth.Application.Clients.Services;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Test.Common.Builders;
using IAuthorizationHandler = OpenAuth.Application.OAuth.Authorization.Handlers.IAuthorizationHandler;

namespace OpenAuth.Test.Integration.Infrastructure.Clients;

public class InternalOAuthClient
{
    public string ApplicationType { get; }
    public string Id { get; }
    public string? Secret { get; }
    public IReadOnlyCollection<string> RedirectUris { get; }

    public AuthorizationGrant AuthorizationGrant;
    

    private readonly IAuthorizationHandler _authorizationHandler;
    private readonly ITokenService _tokenService;
    
    public InternalOAuthClient(IAuthorizationHandler authorizationHandler, ITokenService tokenService, RegisteredClientResponse registered)
    {
        _authorizationHandler = authorizationHandler;
        _tokenService = tokenService;
        
        var client = registered.Client;
        Id = client.Id.ToString();
        ApplicationType = client.ApplicationType.Name;
        RedirectUris = client.RedirectUris
            .Select(uri => uri.ToString())
            .ToArray();
        
        Secret = registered.ClientSecret;
    }

    
    public async Task AuthorizeAsync(Action<AuthorizeCommandBuilder>? config = null)
    {
        var builder = new AuthorizeCommandBuilder()
            .WithClientId(Id)
            .WithRedirectUri(RedirectUris.First());
        
        config?.Invoke(builder);

        var command = builder.Build();

        AuthorizationGrant = await _authorizationHandler.AuthorizeAsync(command);
    }

    public async Task<TokenGenerationResponse> ExchangeCodeForTokenAsync(Action<AuthorizationCodeTokenRequestBuilder>? config = null)
    {
        if (AuthorizationGrant is null)
            throw new InvalidOperationException("No authorization grant available. Call AuthorizeAsync first.");
 
        var builder = new AuthorizationCodeTokenRequestBuilder()
            .WithClientId(Id)
            .FromAuthorizationGrant(AuthorizationGrant)
            .WithClientSecret(Secret);

        config?.Invoke(builder);

        var request = builder.Build();
        return await _tokenService.IssueToken(request);
    }

    public async Task<TokenGenerationResponse> RequestClientCredentialsTokenAsync(Action<ClientCredentialsTokenRequestBuilder>? config = null)
    {
        if (string.IsNullOrWhiteSpace(Secret))
            throw new InvalidOperationException("No secret provided. Client must be public.");
        
        var builder = new ClientCredentialsTokenRequestBuilder()
            .WithClientId(Id)
            .WithClientSecret(Secret);

        config?.Invoke(builder);

        var request = builder.Build();
        return await _tokenService.IssueToken(request);
    }
}