using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Audiences.Interfaces;

namespace OpenAuth.Application.ApiResources.Commands.AddApiResourcePermissions;

public class AddApiResourcePermissionsHandler(IApiResourceRepository repository, IUnitOfWork persistence)
	: ICommandHandler<AddApiResourcePermissionCommand>
{
	public async Task HandleAsync(AddApiResourcePermissionCommand command, CancellationToken ct)
	{
		var apiResource = await repository.GetByIdAsync(command.Id, ct)
		    ?? throw new ApiResourceNotFoundException(command.Id);
		
		apiResource.AddPermissions(command.Permissions);
		
		await persistence.SaveChangesAsync(ct);
	}
}