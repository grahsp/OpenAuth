using OpenAuth.Application.Abstractions;
using OpenAuth.Application.ApiResources;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Services;

namespace OpenAuth.Application.Clients.Commands.CreateClient;

public sealed class CreateM2MClientCommandHandler(
	IClientRepository clientRepository,
	IApiResourceRepository apiResourceRepository,
	ISecretHashProvider hasher,
	IUnitOfWork persistence,
	TimeProvider time)
	: ICommandHandler<CreateM2MClientCommand, CreateClientResult>
{
	public async Task<CreateClientResult> HandleAsync(CreateM2MClientCommand command, CancellationToken ct)
	{
		var now = time.GetUtcNow();
		
		var api = await apiResourceRepository.GetByIdAsync(command.ApiResourceId, ct)
		          ?? throw new ApiResourceNotFoundException(command.ApiResourceId);
		
		var allowedScopes = api.GetScopes();
		if (!command.Scopes.IsSubsetOf(allowedScopes))
			throw new InvalidScopeException("One or more scopes requested are not allowed for the requested API.");
		
		var client = Client.CreateM2M(command.Name, command.ApiResourceId, command.Scopes, now);
		
		var secret = hasher.Create();
		client.AddSecret(secret.Hash, now);
        
		clientRepository.Add(client);
		await persistence.SaveChangesAsync(ct);
        
		var response = new CreateClientResult(client, secret.PlainTextSecret);
		return response;
	}
}