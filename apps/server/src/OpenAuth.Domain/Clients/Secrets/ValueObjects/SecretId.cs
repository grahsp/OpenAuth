namespace OpenAuth.Domain.Clients.Secrets.ValueObjects;

public readonly record struct SecretId(Guid Value)
{
    public static SecretId New() => new SecretId(Guid.NewGuid());

    public static SecretId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SecretId cannot be empty");
        
        return new SecretId(value);
    }
    
    public static SecretId Parse(string value)
    {
        if (!TryParse(value, out var id))
            throw new ArgumentException($"Invalid client ID: {value}", nameof(value));

        return id;
    }

    public static bool TryParse(string? value,  out SecretId id)
    {
        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
        {
            id = default;
            return false;
        }
        
        id = new SecretId(guid);
        return true;
    }
    
    public static implicit operator Guid(SecretId id) => id.Value;    
    public override string ToString() => Value.ToString();
}