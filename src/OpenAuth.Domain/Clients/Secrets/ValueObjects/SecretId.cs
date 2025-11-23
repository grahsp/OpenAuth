using System.Diagnostics.CodeAnalysis;
using OpenAuth.Domain.Shared.Interfaces;

namespace OpenAuth.Domain.Clients.Secrets.ValueObjects;

public record SecretId(Guid Value) : ICreate<string, SecretId>
{
    public static SecretId New() => new(Guid.NewGuid());
    
    public static SecretId Create(string value)
    {
        if (!TryCreate(value, out var id))
            throw new ArgumentException($"Invalid client ID: {value}", nameof(value));

        return id;
    }

    public static bool TryCreate(string? value, [NotNullWhen(true)] out SecretId? id)
    {
        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
        {
            id = null;
            return false;
        }
        
        id = new SecretId(guid);
        return true;
    }
    
    public override string ToString() => Value.ToString();
}