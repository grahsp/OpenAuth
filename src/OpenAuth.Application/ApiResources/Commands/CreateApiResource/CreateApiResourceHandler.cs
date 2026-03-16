using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Domain.Apis;

namespace OpenAuth.Application.ApiResources.Commands.CreateApiResource;

public class CreateApiResourceHandler(IApiResourceRepository repository, IUnitOfWork persistence)
	: ICommandHandler<CreateApiResourceCommand, CreateApiResourceResult>
{
	public async Task<CreateApiResourceResult> HandleAsync(CreateApiResourceCommand command, CancellationToken ct)
	{
		var apiResource = ApiResource.Create(
			command.ResourceName,
			command.AudienceIdentifier,
			command.Permissions);
		
		repository.Add(apiResource);
		await persistence.SaveChangesAsync(ct);

		return new CreateApiResourceResult(apiResource.Id);
	}
}