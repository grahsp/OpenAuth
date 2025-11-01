using OpenAuth.Domain.Shared.ValueObjects;

namespace OpenAuth.Domain.Clients.Audiences.ValueObjects;

public sealed record Scope : Name
{
    public Scope(string value) : base(value, 2, 32) { }

    public static Scope[] ParseMany(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];
        
        return value.Split(' ')
            .Select(v => new Scope(v.Trim()))
            .ToArray();
    }
    
    public override string ToString() => Value;
}