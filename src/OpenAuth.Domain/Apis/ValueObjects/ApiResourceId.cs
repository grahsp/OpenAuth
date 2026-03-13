using System.Diagnostics.CodeAnalysis;

namespace OpenAuth.Domain.Apis.ValueObjects;

public sealed record ApiResourceId(Guid Value)
{
    public static ApiResourceId New() => new ApiResourceId(Guid.NewGuid());
    
    public static ApiResourceId Create(string value)
    {
        if (!TryCreate(value, out var id))
            throw new ArgumentException($"Invalid client ID: {value}", nameof(value));

        return id;
    }
    
    public static bool TryCreate(string? value, [NotNullWhen(true)] out ApiResourceId? id)
    {
        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
        {
            id = null;
            return false;
        }
        
        id = new ApiResourceId(guid);
        return true;
    }
    
    public override string ToString() => Value.ToString();
}