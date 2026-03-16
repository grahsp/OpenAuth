namespace OpenAuth.Domain.Clients.ValueObjects;

public readonly record struct ClientId(Guid Value)
{
    public static ClientId New() => new ClientId(Guid.NewGuid());

    public static ClientId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty");

        return new ClientId(value);
    }

    public static ClientId Parse(string value)
    {
        if (!TryParse(value, out var id))
            throw new ArgumentException($"Invalid client ID: {value}", nameof(value));

        return id;
    }

    public static bool TryParse(string? value, out ClientId id)
    {
        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
        {
            id = default;
            return false;
        }
        
        id = new ClientId(guid);
        return true;
    }
    
    public static implicit operator Guid(ClientId id) => id.Value;    
    public override string ToString() => Value.ToString();
}