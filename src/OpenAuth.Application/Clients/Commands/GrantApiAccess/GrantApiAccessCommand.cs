using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Commands.GrantApiAccess;

public sealed record GrantApiAccessCommand(
	ClientId ClientId,
	ApiResourceId ApiResourceId,
	ScopeCollection Scopes
) : ICommand;