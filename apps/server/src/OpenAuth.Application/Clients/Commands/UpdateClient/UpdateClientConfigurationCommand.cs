using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Commands.UpdateClient;

public sealed record UpdateClientConfigurationCommand(
	ClientId Id,
	ClientName Name,
	ClientApplicationType ApplicationType,
	IEnumerable<RedirectUri> RedirectUris,
	TimeSpan TokenLifetime,
	IEnumerable<GrantType> AllowedGrantTypes
) : ICommand;