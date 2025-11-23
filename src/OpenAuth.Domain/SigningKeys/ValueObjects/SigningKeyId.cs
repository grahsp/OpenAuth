using System.Diagnostics.CodeAnalysis;
using OpenAuth.Domain.Shared.Interfaces;

namespace OpenAuth.Domain.SigningKeys.ValueObjects;

public record SigningKeyId(Guid Value) : ICreate<string, SigningKeyId>
{
    public static SigningKeyId New() => new(Guid.NewGuid());
    
    public static SigningKeyId Create(string value)
    {
        if (!TryCreate(value, out var id))
            throw new ArgumentException($"Invalid client ID: {value}", nameof(value));

        return id;
    }

    public static bool TryCreate(string value, [NotNullWhen(true)] out SigningKeyId? id)
    {
        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
        {
            id = null;
            return false;
        }
        
        id = new SigningKeyId(guid);
        return true;
    }
    
    public override string ToString() => Value.ToString();
}