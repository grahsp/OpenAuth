using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Secrets.Mappings;
using OpenAuth.Domain.Clients;

namespace OpenAuth.Application.Clients.Mappings;

public static class ClientMappingExtensions
{
	public static ClientInfo ToClientInfo(this Client client)
		=> new ClientInfo(
			client.Id.ToString(),
			client.Name.ToString(),
			client.CreatedAt,
			client.UpdatedAt
		);

	public static ClientDetails ToClientDetails(this Client client)
		=> new ClientDetails(
			client.Id,
			client.Name,
			client.CreatedAt,
			client.UpdatedAt,
			client.Secrets
				.OrderByDescending(s => s.CreatedAt)
				.Select(s => s.ToSecretInfo()),
			client.Apis
				.Select(a => a.ToClientApiAccessDetails()).ToArray()
		);
}

public static class ClientApiAccessMappingExtensions
{
	public static ClientApiAccessDetails ToClientApiAccessDetails(this ClientApiAccess clientApiAccess)
		=> new ClientApiAccessDetails(clientApiAccess.ApiResourceId.ToString(), clientApiAccess.AllowedScopes.ToString());
}

public sealed record ClientApiAccessDetails(string ApiResourceId, string GrantedScopes);