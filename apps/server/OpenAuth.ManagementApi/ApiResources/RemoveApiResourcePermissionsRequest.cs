namespace OpenAuth.ManagementApi.ApiResources;

public sealed record RemoveApiResourcePermissionsRequest(List<string> Scopes);