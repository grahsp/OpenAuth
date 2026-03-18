using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Commands.CreateClient;

public sealed record CreateClientCommand(
	ClientApplicationType Type,
	ClientName Name,
	ApiResourceId? ApiResourceId,
	ScopeCollection? Scopes
) : ICommand<CreateClientResult>;