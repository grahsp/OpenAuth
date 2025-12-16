using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Apis;

public class Api
{
    public ApiId Id { get; private init; }
    public ApiName Name { get; private init; }
    
    public AudienceIdentifier Audience { get; private init; }
    
    private readonly Dictionary<Scope, Permission> _permissions;
    public IReadOnlyCollection<Permission> Permissions => _permissions.Values;
    
    private Api() { }

    private Api(ApiName name, AudienceIdentifier audience, IEnumerable<Permission> permissions)
    {
        Id = ApiId.New();
        Name = name;
        Audience = audience;
        _permissions = permissions.ToDictionary(k => k.Scope, v => v);
    }

    internal static Api Create(ApiName name, AudienceIdentifier audience, IEnumerable<Permission> permissions)
        => new(name, audience, permissions);
}