using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Apis.ValueObjects;

public sealed record Permission(Scope Scope, ScopeDescription Description)
{
    public bool Equals(Permission? other) =>
        other is not null && Scope.Equals(other.Scope);

    public override int GetHashCode() =>
        Scope.GetHashCode();
}