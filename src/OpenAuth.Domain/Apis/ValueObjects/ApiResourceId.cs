namespace OpenAuth.Domain.Apis.ValueObjects;

public readonly record struct ApiResourceId(Guid Value)
{
    public static ApiResourceId New() => new ApiResourceId(Guid.NewGuid());

    public static ApiResourceId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ApiResourceId cannot be empty");
        
        return new ApiResourceId(value);
    }
    
    public static ApiResourceId Parse(string value)
    {
        if (!TryParse(value, out var id))
            throw new ArgumentException($"Invalid client ID: {value}", nameof(value));

        return id;
    }
    
    public static bool TryParse(string? value, out ApiResourceId id)
    {
        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
        {
            id = default;
            return false;
        }
        
        id = new ApiResourceId(guid);
        return true;
    }
    
    public static implicit operator Guid(ApiResourceId id) => id.Value;    
    public override string ToString() => Value.ToString();
}