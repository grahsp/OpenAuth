using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Interfaces;

namespace OpenAuth.Application.Clients.Commands.UpdateClient;

public class UpdateClientConfigurationCommandHandler(IClientRepository repository, IUnitOfWork persistence, TimeProvider time) : ICommandHandler<UpdateClientConfigurationCommand>
{
	public async Task HandleAsync(UpdateClientConfigurationCommand configurationCommand, CancellationToken ct)
	{
		var client = await repository.GetByIdAsync(configurationCommand.Id, ct)
			?? throw new ClientNotFoundException(configurationCommand.Id);

		var now = time.GetUtcNow();
		
		client.Rename(configurationCommand.Name, now);
		client.SetRedirectUris(configurationCommand.RedirectUris, now);
		client.SetGrantTypes(configurationCommand.AllowedGrantTypes, now);
		client.SetTokenLifetime(configurationCommand.TokenLifetime, now);

		await persistence.SaveChangesAsync(ct);
	}
}