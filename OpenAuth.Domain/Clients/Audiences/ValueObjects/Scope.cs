using OpenAuth.Domain.Shared.ValueObjects;

namespace OpenAuth.Domain.Clients.Audiences.ValueObjects;

public sealed record Scope : Name
{
    public Scope(string value) :
        base(value.Trim().ToLower(), 2, 32) { }
    
    public override string ToString() => Value;
}