using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Audiences.Interfaces;

namespace OpenAuth.Application.ApiResources.Commands.RemoveApiResourcePermissions;

public class RemoveApiResourcePermissionsHandler(IApiResourceRepository repository, IUnitOfWork persistence)
	: ICommandHandler<RemoveApiResourcePermissionsCommand>
{
	public async Task HandleAsync(RemoveApiResourcePermissionsCommand command, CancellationToken ct)
	{
		var apiResource = await repository.GetByIdAsync(command.Id, ct)
			?? throw new ApiResourceNotFoundException(command.Id);
		
		apiResource.RemovePermissions(command.Scopes);
		await persistence.SaveChangesAsync(ct);
	}
}