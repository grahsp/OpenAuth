using OpenAuth.Application.Clients.Commands.CreateClient;

namespace OpenAuth.Server.Clients;

public static class ClientMapper
{
	public static CreateClientResponse ToResponse(this CreateClientResult result)
	{
		return new CreateClientResponse(result.Client.Id.ToString(), result.PlainTextSecret);
	}
}