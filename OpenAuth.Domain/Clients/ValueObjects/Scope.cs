using OpenAuth.Domain.Shared.ValueObjects;

namespace OpenAuth.Domain.Clients.ValueObjects;

public sealed record Scope : Name
{
    public Scope(string value) :
        base(value, 2, 32) { }
    
    public override string ToString() => NormalizedValue;
}