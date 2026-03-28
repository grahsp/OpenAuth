namespace OpenAuth.Server.Management.ApiResources;

public sealed record AddApiResourcePermissionsRequest(Dictionary<string, string?> Permissions);