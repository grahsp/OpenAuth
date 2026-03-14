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
		var name = new ApiName(_name);
		var audience = new AudienceIdentifier(_audience);

		if (_permissions.Count == 0)
			WithScopes(DefaultValues.Scopes);
			
		return ApiResource.Create(name, audience, _permissions.Values);
	}
}