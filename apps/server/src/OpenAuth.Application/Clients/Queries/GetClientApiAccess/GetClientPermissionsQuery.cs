using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Queries.GetClientApiAccess;

public sealed record GetClientPermissionsQuery(ClientId ClientId)
	: IQuery<IReadOnlyList<ClientPermissions>>;