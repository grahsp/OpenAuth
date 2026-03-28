using OpenAuth.Application.Shared;
using OpenAuth.Domain.ApiResources.ValueObjects;

namespace OpenAuth.Application.ApiResources;

public sealed class ApiResourceNotFoundException : NotFoundException
{
	public ApiResourceNotFoundException(ApiResourceId apiResourceId)
		: base($"API resource with ID '{apiResourceId}' not found.")
	{}

	public ApiResourceNotFoundException(AudienceIdentifier audience)
		: base($"API resource with Audience '${audience}' not found.")
	{}
}
