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

    public AuthorizationGrant? AuthorizationGrant;
    

    private readonly IAuthorizationHandler _authorizationHandler;
    private readonly ITokenRequestHandler _tokenRequestHandler;
    
    public InternalOAuthClient(IAuthorizationHandler authorizationHandler, ITokenRequestHandler tokenRequestHandler, RegisteredClientResponse registered)
    {
        _authorizationHandler = authorizationHandler;
        _tokenRequestHandler = tokenRequestHandler;
        
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

    public async Task<TokenResult> ExchangeCodeForTokenAsync(Action<AuthorizationCodeTokenCommandBuilder>? config = null)
    {
        if (AuthorizationGrant is null)
            throw new InvalidOperationException("No authorization grant available. Call AuthorizeAsync first.");
 
        var builder = new AuthorizationCodeTokenCommandBuilder()
            .WithClientId(Id)
            .FromAuthorizationGrant(AuthorizationGrant)
            .WithClientSecret(Secret);

        config?.Invoke(builder);

        var request = builder.Build();
        return await _tokenRequestHandler.HandleAsync(request);
    }

    public async Task<TokenResult> RequestClientCredentialsTokenAsync(Action<ClientCredentialsTokenCommandBuilder>? config = null)
    {
        if (string.IsNullOrWhiteSpace(Secret))
            throw new InvalidOperationException("No secret provided. Client must be public.");
        
        var builder = new ClientCredentialsTokenCommandBuilder()
            .WithClientId(Id)
            .WithClientSecret(Secret);

        config?.Invoke(builder);

        var request = builder.Build();
        return await _tokenRequestHandler.HandleAsync(request);
    }
}