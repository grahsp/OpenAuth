using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.ApiResources;

public class ApiResource
{
	public ApiResourceId Id { get; private init; }
	public ApiResourceName ResourceName { get; private init; }
    
	public AudienceIdentifier Audience { get; private init; }
    
	private readonly HashSet<Permission> _permissions = [];
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

	public ScopeCollection GetScopes()
	{
		var scopes = Permissions.Select(p => p.Scope);
		return new ScopeCollection(scopes);
	}

	public void ValidateScopes(ScopeCollection scopes)
	{
		var allowedScopes = Permissions.Select(p => p.Scope);
		if (!scopes.IsSubsetOf(allowedScopes))
			throw new InvalidOperationException("One or more scopes requested are not allowed for this API.");
	}

	public void AddPermission(Permission permission)
	{
		if (_permissions.Contains(permission))
			throw new DuplicatePermissionException(permission);

		_permissions.Add(permission);
	}

	public void AddPermissions(IEnumerable<Permission> permissions)
	{
		foreach (var permission in permissions)
			AddPermission(permission);
	}

	public void RemovePermission(Scope scope)
	{
		_permissions.RemoveWhere(x => x.Scope == scope);
	}

	public void RemovePermissions(IEnumerable<Scope> scopes)
	{
		foreach (var scope in scopes)
			RemovePermission(scope);
	}
}