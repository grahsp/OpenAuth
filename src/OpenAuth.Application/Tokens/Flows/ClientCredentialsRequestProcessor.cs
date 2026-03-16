using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public class ClientCredentialsRequestProcessor : TokenRequestProcessor<ClientCredentialsTokenCommand>
{
	public override GrantType GrantType => GrantType.ClientCredentials;
    
	private readonly IApiResourceRepository _apiResourceRepository;
	private readonly ISecretQueryService _secretQueryService;
    
	public ClientCredentialsRequestProcessor(IApiResourceRepository apiResourceRepository, ISecretQueryService secretQueryService)
	{
		_apiResourceRepository = apiResourceRepository;
		_secretQueryService = secretQueryService;
	}

	protected override async Task<TokenContext> ProcessAsync(ClientCredentialsTokenCommand command, ClientTokenData tokenData, CancellationToken ct = default)
	{
		if (string.IsNullOrWhiteSpace(command.ClientSecret))
			throw new InvalidRequestException("ClientSecret is required.");
        
		if (command.RequestedScopes is null)
			throw new InvalidRequestException("Scopes is required.");

		var api = await _apiResourceRepository.GetByAudienceAsync(command.Audience, ct)
			?? throw new InvalidOperationException("API not found.");
		
		var apiScopes = api.Permissions.Select(p => p.Scope).ToArray();
		command.RequestedScopes.IsSubsetOf(apiScopes);
        
		if (!await _secretQueryService.ValidateSecretAsync(command.ClientId, command.ClientSecret, ct))
			throw new InvalidClientException("Invalid client credentials.");

		return new TokenContext(
			command.RequestedScopes,
			command.ClientId.ToString(),
			command.Audience
		);
	}
}