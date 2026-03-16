using OpenAuth.Domain.ApiResources.ValueObjects;

namespace OpenAuth.Domain.ApiResources;

public sealed class DuplicatePermissionException(Permission permission) : Exception($"API resource already has permission '{permission.Scope}'");