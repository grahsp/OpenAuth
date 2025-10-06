namespace OpenAuth.Domain.Clients.ValueObjects;

public record ClientName
{
    public const int MinLength = 3, MaxLength = 24;
    public string Value { get; }
    
    public ClientName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentOutOfRangeException.ThrowIfLessThan(value.Length, MinLength, nameof(value));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, MaxLength, nameof(value));
        
        Value = value.Trim();
    }
    
    public override string ToString()
        => Value;
}