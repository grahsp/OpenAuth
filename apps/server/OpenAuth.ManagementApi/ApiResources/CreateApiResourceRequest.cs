namespace OpenAuth.ManagementApi.ApiResources;

public sealed record CreateApiResourceRequest(string Name, string AudienceIdentifier, Dictionary<string, string?> Permissions);