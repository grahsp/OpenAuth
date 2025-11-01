using System.Diagnostics.CodeAnalysis;
using OpenAuth.Domain.Shared.Interfaces;

namespace OpenAuth.Domain.Clients.Audiences.ValueObjects;

public record AudienceId(Guid Value) : ICreate<string, AudienceId>
{
    public static AudienceId New() => new(Guid.NewGuid());
    
    public static AudienceId Create(string value)
    {
        if (!TryCreate(value, out var id))
            throw new ArgumentException($"Invalid client ID: {value}", nameof(value));

        return id;
    }

    public static bool TryCreate(string value, [NotNullWhen(true)] out AudienceId? id)
    {
        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
        {
            id = null;
            return false;
        }
        
        id = new AudienceId(guid);
        return true;
    }
    
    public override string ToString() => Value.ToString();
}