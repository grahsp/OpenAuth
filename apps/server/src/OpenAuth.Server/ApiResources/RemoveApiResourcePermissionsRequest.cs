namespace OpenAuth.Server.ApiResources;

public sealed record RemoveApiResourcePermissionsRequest(List<string> Scopes);