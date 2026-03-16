namespace OpenAuth.ManagementApi.ApiResources;

public sealed record AddApiResourcePermissionsRequest(Dictionary<string, string?> Permissions);