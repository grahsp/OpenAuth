namespace OpenAuth.Server.Management.ApiResources;

public sealed record RemoveApiResourcePermissionsRequest(List<string> Scopes);