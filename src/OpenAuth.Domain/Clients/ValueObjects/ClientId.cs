using System.Diagnostics.CodeAnalysis;
using OpenAuth.Domain.Shared.Interfaces;

namespace OpenAuth.Domain.Clients.ValueObjects;

public record ClientId(Guid Value) : ICreate<string, ClientId>
{
    public static ClientId New() => new(Guid.NewGuid());

    public static ClientId Create(string value)
    {
        if (!TryCreate(value, out var id))
            throw new ArgumentException($"Invalid client ID: {value}", nameof(value));

        return id;
    }

    public static bool TryCreate(string? value, [NotNullWhen(true)] out ClientId? id)
    {
        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
        {
            id = null;
            return false;
        }
        
        id = new ClientId(guid);
        return true;
    }
    
    public override string ToString() => Value.ToString();
}