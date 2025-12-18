using OpenAuth.Domain.Apis;
using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class ApiBuilder
{
    private ApiName? _name;
    private AudienceIdentifier? _identifier;
    private Dictionary<string, Permission> _permissions = [];

    public ApiBuilder WithName(string name)
    {
        _name = new ApiName(name);
        return this;
    }

    public ApiBuilder WithIdentifier(string identifier)
    {
        _identifier = new AudienceIdentifier(identifier);
        return this;
    }

    public ApiBuilder WithPermission(string scope, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (_permissions.ContainsKey(scope))
            throw new InvalidOperationException($"Permissions '{scope}' already exists.");
        
        var permission = new Permission(
            new Scope(scope),
            new ScopeDescription(description)
        );
        
        _permissions[scope] = permission;
        return this;
    }

    public Api Build()
    {
        var name = _name ?? new ApiName("test-api");
        var identifier = _identifier ?? new AudienceIdentifier($"https://example.com/{Guid.NewGuid()}");

        try
        {
            return Api.Create(name, identifier, _permissions.Values);
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to build API. Builder produced invalid state.", ex);
        }
    }
}