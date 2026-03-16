using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Commands.RevokeApiAccess;

public sealed record RevokeApiAccessCommand(
	ClientId ClientId,
	ApiResourceId ApiResourceId
) : ICommand;