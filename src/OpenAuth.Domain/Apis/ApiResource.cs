using OpenAuth.Domain.Apis.ValueObjects;

namespace OpenAuth.Domain.Apis;

public class ApiResource
{
    public ApiResourceId Id { get; private init; }
    public ApiName Name { get; private init; }
    
    public AudienceIdentifier Audience { get; private init; }
    
    private readonly HashSet<Permission> _permissions;
    public IReadOnlyCollection<Permission> Permissions => _permissions;
    
    private ApiResource() { }

    private ApiResource(ApiName name, AudienceIdentifier audience, IEnumerable<Permission> permissions)
    {
        Id = ApiResourceId.New();
        Name = name;
        Audience = audience;
        _permissions = permissions.ToHashSet();
    }

    internal static ApiResource Create(ApiName name, AudienceIdentifier audience, IEnumerable<Permission> permissions)
    {
        var permissionSet = permissions.ToHashSet();
        
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(permissionSet.Count, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(permissionSet.Count, 50);
        
        return new ApiResource(name, audience, permissionSet);
    }
}