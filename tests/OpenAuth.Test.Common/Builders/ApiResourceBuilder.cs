using OpenAuth.Domain.Apis;
using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public class ApiResourceBuilder
{
	private string _name = DefaultValues.ApiName;
	private string _audience = DefaultValues.ApiAudience;
	private readonly Dictionary<string, Permission> _permissions = [];

	public ApiResourceBuilder WithName(string name)
	{
		_name = name;
		return this;
	}

	public ApiResourceBuilder WithAudience(string audience)
	{
		_audience = audience;
		return this;
	}

	public ApiResourceBuilder WithScopes(string scopes)
	{
		var collection = ScopeCollection.Parse(scopes);

		foreach (var scope in collection)
			WithPermission(scope.Value);

		return this;
	}

	public ApiResourceBuilder WithPermission(string scope, string description = "description")
	{
		if (_permissions.ContainsKey(scope))
			throw new InvalidOperationException($"Permissions '{scope}' already exists.");
        
		var permission = new Permission(
			new Scope(scope),
			new ScopeDescription(description)
		);
        
		_permissions[scope] = permission;
		return this;
	}

	public ApiResource Build()
	{
		var name = new ApiResourceName(_name);
		var audience = new AudienceIdentifier(_audience);
		
		var permissions = _permissions.Values.ToList();

		if (permissions.Count == 0)
		{
			var defaults = ScopeCollection.Parse(DefaultValues.Scopes);
			
			permissions = defaults.Select(scope => new Permission(
				scope,
				new ScopeDescription(scope.Value))
			).ToList();
		}
			
		return ApiResource.Create(name, audience, permissions);
	}
}