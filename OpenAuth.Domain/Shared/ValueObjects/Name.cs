namespace OpenAuth.Domain.Shared.ValueObjects;

public abstract record Name
{
    public string Value { get; }
    public string NormalizedValue => Normalize(Value);

    protected Name(string value, int min, int max)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var trimmed = value.Trim();
        ArgumentOutOfRangeException.ThrowIfLessThan(trimmed.Length, min, nameof(value));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length,max, nameof(value));

        Value = trimmed;
    }

    protected virtual string Normalize(string value)
        => value.ToLowerInvariant();

    public override string ToString() => Value;
}