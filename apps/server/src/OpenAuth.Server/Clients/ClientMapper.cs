using OpenAuth.Application.Clients.Commands.CreateClient;

namespace OpenAuth.AuthorizationApi.Clients;

public static class ClientMapper
{
	public static CreateClientResponse ToResponse(this CreateClientResult result)
	{
		return new CreateClientResponse(result.Client.Id.ToString(), result.PlainTextSecret);
	}
}