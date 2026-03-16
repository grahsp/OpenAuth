namespace OpenAuth.ManagementApi.ApiResources;

public sealed record AddApiResourcePermissionRequest(Dictionary<string, string?> Permissions);