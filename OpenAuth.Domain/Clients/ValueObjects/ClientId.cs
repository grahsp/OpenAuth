namespace OpenAuth.Domain.Clients.ValueObjects;

public readonly record struct ClientId(Guid Value)
{
    public static ClientId New() => new(Guid.NewGuid());

    public static bool TryCreate(string value, out ClientId clientId)
    {
        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
        {
            clientId = default;
            return false;
        }
        
        clientId = new ClientId(guid);
        return true;
    }
    
    public override string ToString() => Value.ToString();
}