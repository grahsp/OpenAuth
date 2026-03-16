using OpenAuth.Domain.Apis.ValueObjects;

namespace OpenAuth.Domain.Apis;

public class ApiResource
{
    public ApiResourceId Id { get; private init; }
    public ApiResourceName ResourceName { get; private init; }
    
    public AudienceIdentifier Audience { get; private init; }
    
    private readonly HashSet<Permission> _permissions;
    public IReadOnlyCollection<Permission> Permissions => _permissions;
    
    private ApiResource() { }

    private ApiResource(ApiResourceName resourceName, AudienceIdentifier audience, IEnumerable<Permission> permissions)
    {
        Id = ApiResourceId.New();
        ResourceName = resourceName;
        Audience = audience;
        _permissions = permissions.ToHashSet();
    }

    internal static ApiResource Create(ApiResourceName resourceName, AudienceIdentifier audience, IEnumerable<Permission> permissions)
    {
        var permissionSet = permissions.ToHashSet();
        
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(permissionSet.Count, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(permissionSet.Count, 50);
        
        return new ApiResource(resourceName, audience, permissionSet);
    }
}