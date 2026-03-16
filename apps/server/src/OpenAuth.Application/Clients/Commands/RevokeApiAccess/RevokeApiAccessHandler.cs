using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Application.Clients.Interfaces;

namespace OpenAuth.Application.Clients.Commands.RevokeApiAccess;

public class RevokeApiAccessHandler(IClientRepository clientRepository, TimeProvider time)
	: ICommandHandler<RevokeApiAccessCommand>
{
	public async Task HandleAsync(RevokeApiAccessCommand command, CancellationToken ct)
	{
		var client = await clientRepository.GetByIdAsync(command.ClientId, ct)
		    ?? throw new ClientNotFoundException(command.ClientId);
        
		client.RevokeApiAccess(command.ApiResourceId, time.GetUtcNow());
		await clientRepository.SaveChangesAsync(ct);
	}
}