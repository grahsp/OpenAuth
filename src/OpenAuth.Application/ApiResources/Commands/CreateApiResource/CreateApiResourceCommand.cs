using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.Apis.ValueObjects;

namespace OpenAuth.Application.ApiResources.Commands.CreateApiResource;

public sealed record CreateApiResourceCommand(
	ApiResourceName ResourceName,
	AudienceIdentifier AudienceIdentifier,
	IEnumerable<Permission> Permissions
) : ICommand<CreateApiResourceResult>;