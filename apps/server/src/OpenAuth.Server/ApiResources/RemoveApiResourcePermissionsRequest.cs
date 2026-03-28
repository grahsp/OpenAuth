namespace OpenAuth.AuthorizationApi.ApiResources;

public sealed record RemoveApiResourcePermissionsRequest(List<string> Scopes);