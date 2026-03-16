using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Apis.ValueObjects;

public sealed class Permission
{
    public Scope Scope { get; }
    public ScopeDescription Description { get; }
    
    private Permission() { }
    
    public Permission(Scope scope, ScopeDescription description)
    {
        Scope = scope;
        Description = description;
    }
}