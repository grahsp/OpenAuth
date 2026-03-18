using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Services;

namespace OpenAuth.Application.Clients.Commands.CreateClient;

public sealed class CreateWebClientCommandHandler(
	IClientRepository repository,
	ISecretHashProvider hasher,
	IUnitOfWork persistence,
	TimeProvider time)
	: ICommandHandler<CreateWebClientCommand, CreateClientResult>
{
	public async Task<CreateClientResult> HandleAsync(CreateWebClientCommand command, CancellationToken ct)
	{
		var now = time.GetUtcNow();
		
		var client = Client.CreateWeb(command.Name, now);

		var secret = hasher.Create();
		client.AddSecret(secret.Hash, now);
        
		repository.Add(client);
		await persistence.SaveChangesAsync(ct);
        
		var response = new CreateClientResult(client, secret.PlainTextSecret);
		return response;
	}
}