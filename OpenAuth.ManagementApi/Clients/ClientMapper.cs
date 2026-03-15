using OpenAuth.Application.Clients.Services;

namespace OpenAuth.ManagementApi.Clients;

public static class ClientMapper
{
	public static ClientCreationResponse ToResponse(this RegisteredClientResponse result)
	{
		return new ClientCreationResponse(result.Client.Id.ToString(), result.ClientSecret);
	}
}