using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Audiences.Interfaces;

namespace OpenAuth.Application.ApiResources.Commands.DeleteApiResource;

public class DeleteApiResourceHandler(IApiResourceRepository repository, IUnitOfWork persistence)
	: ICommandHandler<DeleteApiResourceCommand>
{
	public async Task HandleAsync(DeleteApiResourceCommand command, CancellationToken ct)
	{
		var apiResource = await repository.GetByIdAsync(command.Id, ct);

		if (apiResource is null)
			return;
		
		repository.Remove(apiResource);
		await persistence.SaveChangesAsync(ct);
	}
}