using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Commands.SetClientApiAccess;

public sealed record SetClientApiAccessCommand(
	ClientId ClientId,
	ApiResourceId ApiResourceId,
	ScopeCollection Scopes
) : ICommand;