namespace OpenAuth.Server.ApiResources;

public sealed record AddApiResourcePermissionsRequest(Dictionary<string, string?> Permissions);