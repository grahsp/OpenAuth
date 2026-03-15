using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;

namespace OpenAuth.Test.Integration.OAuth;

public class InternalOAuthModule : IAsyncDisposable
{
	public string ApplicationType { get; }
	public ClientId Id { get; }
	public string? Secret { get; }
	public IReadOnlyCollection<string> RedirectUris { get; }

	public AuthorizationGrant? AuthorizationGrant;


	private readonly IServiceScope _scope;
	private readonly IAuthorizationHandler _authorizationHandler;
	private readonly ITokenRequestHandler _tokenRequestHandler;
    
	public InternalOAuthModule(IServiceScope scope, RegisteredClientResponse registered)
	{
		_scope = scope;
		
		var services = scope.ServiceProvider;
		_authorizationHandler = services.GetRequiredService<IAuthorizationHandler>();
		_tokenRequestHandler = services.GetRequiredService<ITokenRequestHandler>();
        
		var client = registered.Client;
		Id = client.Id;
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

		AuthorizationGrant = await _authorizationHandler.HandleAsync(command);
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

	public ValueTask DisposeAsync()
	{
		_scope.Dispose();   
		return ValueTask.CompletedTask;
	}
}