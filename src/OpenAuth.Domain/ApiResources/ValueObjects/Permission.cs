using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.ApiResources.ValueObjects;

public sealed record Permission(Scope Scope, ScopeDescription? Description)
{
    public static Permission Parse(string scope, string? description) =>
        new Permission(new Scope(scope), new ScopeDescription(description));
    
    public bool Equals(Permission? other) =>
        other is not null && Scope.Equals(other.Scope);

    public override int GetHashCode() =>
        Scope.GetHashCode();
}