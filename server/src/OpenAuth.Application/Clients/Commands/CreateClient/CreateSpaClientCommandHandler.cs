using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Services;

namespace OpenAuth.Application.Clients.Commands.CreateClient;

public sealed class CreateSpaClientCommandHandler(
	IClientRepository repository,
	IUnitOfWork persistence,
	TimeProvider time)
	: ICommandHandler<CreateSpaClientCommand, CreateClientResult>
{
	public async Task<CreateClientResult> HandleAsync(CreateSpaClientCommand command, CancellationToken ct)
	{
		var now = time.GetUtcNow();
		
		var client = Client.CreateSpa(command.Name, now);

		repository.Add(client);
		await persistence.SaveChangesAsync(ct);
        
		var response = new CreateClientResult(client, null);
		return response;
	}
}