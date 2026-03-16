using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Commands.RevokeApiAccess;

public sealed record RevokeApiAccessCommand(
	ClientId ClientId,
	ApiResourceId ApiResourceId
) : ICommand;