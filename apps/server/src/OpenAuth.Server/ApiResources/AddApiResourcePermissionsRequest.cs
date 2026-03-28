namespace OpenAuth.AuthorizationApi.ApiResources;

public sealed record AddApiResourcePermissionsRequest(Dictionary<string, string?> Permissions);