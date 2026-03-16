namespace OpenAuth.Domain.SigningKeys.ValueObjects;

public readonly record struct SigningKeyId(Guid Value)
{
    public static SigningKeyId New() => new SigningKeyId(Guid.NewGuid());

    public static SigningKeyId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SigningKeyId cannot be empty");
        
        return new SigningKeyId(value);    
    }
    
    public static SigningKeyId Parse(string value)
    {
        if (!TryParse(value, out var id))
            throw new ArgumentException($"Invalid client ID: {value}", nameof(value));

        return id;
    }

    public static bool TryParse(string value,  out SigningKeyId id)
    {
        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
        {
            id = default;
            return false;
        }
        
        id = new SigningKeyId(guid);
        return true;
    }
    
    public static implicit operator Guid(SigningKeyId id) => id.Value;    
    public override string ToString() => Value.ToString();
}