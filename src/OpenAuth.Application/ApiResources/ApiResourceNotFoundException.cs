using OpenAuth.Application.Shared;
using OpenAuth.Domain.Apis.ValueObjects;

namespace OpenAuth.Application.ApiResources;

public sealed class ApiResourceNotFoundException(ApiResourceId apiResourceId) : NotFoundException($"API resource with ID '{apiResourceId}' not found.");