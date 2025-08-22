namespace OpenAuth.Domain.ValueObjects;

public readonly record struct Audience
{
    public string Value { get; }
    public const int Min = 3, Max = 16;
    
    public Audience(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToLowerInvariant();
        Value = normalized.Length switch
        {
            < Min => throw new ArgumentOutOfRangeException(nameof(value), $"Value must be greater than {Min} characters long."),
            > Max => throw new ArgumentOutOfRangeException(nameof(value), $"Value must be less than {Max} characters long."),
            _ => normalized
        };
    }
    
    public override string ToString() => Value;
}