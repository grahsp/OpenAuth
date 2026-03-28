using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.Clients.ApplicationType;

namespace OpenAuth.Application.Clients.Commands.CreateClient;

public sealed class CreateClientCommandHandler(
	ICommandHandler<CreateM2MClientCommand, CreateClientResult> m2m,
	ICommandHandler<CreateSpaClientCommand, CreateClientResult> spa,
	ICommandHandler<CreateWebClientCommand, CreateClientResult> web)
	: ICommandHandler<CreateClientCommand, CreateClientResult>
{
	public Task<CreateClientResult> HandleAsync(CreateClientCommand command, CancellationToken ct)
	{
		return command.Type switch
		{
			MachineToMachineClientApplicationType => m2m.HandleAsync(MapToM2M(command), ct),
			SinglePageClientApplicationType => spa.HandleAsync(MapToSpa(command), ct),
			WebApplicationType => web.HandleAsync(MapToWeb(command), ct),
			_ => throw new InvalidOperationException("Unknown application type provided.")
		};
	}
	
	private static CreateM2MClientCommand MapToM2M(CreateClientCommand command)
	{
		if (command.ApiResourceId is null)
			throw new ValidationException("ApiId is required.");
		
		if (command.Scopes is null)
			throw new ValidationException("Scopes is required.");
		
		return new CreateM2MClientCommand(command.Name, command.ApiResourceId.Value, command.Scopes);
	}

	private static CreateSpaClientCommand MapToSpa(CreateClientCommand command)
	{
		return new CreateSpaClientCommand(command.Name);
	}
	
	private static CreateWebClientCommand MapToWeb(CreateClientCommand command)
	{
		return new CreateWebClientCommand(command.Name);
	}
}

public sealed class ValidationException(string message) : Exception(message);