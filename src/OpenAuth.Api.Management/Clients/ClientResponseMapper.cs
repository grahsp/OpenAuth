using OpenAuth.Application.Clients.Services;

namespace OpenAuth.Api.Management.Clients;

public static class ClientResponseMapper
{
    public static CreateClientRequestDto ToResponse(this CreateClientResult result)
        => new(result.Client.Id.ToString(), result.ClientSecret);
}