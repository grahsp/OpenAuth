namespace OpenAuth.Domain.ValueObjects;

public sealed class Audience : IEquatable<Audience>
{
    public const int Min = 3, Max = 128;

    public string Value => _value;
    private readonly string _value;
    
    public IReadOnlyCollection<Scope> Scopes => _scopes;
    private readonly HashSet<Scope> _scopes = [];

    public Audience()
    { }
    
    public Audience(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToLowerInvariant();
        _value = normalized.Length switch
        {
            < Min => throw new ArgumentOutOfRangeException(nameof(value), $"Value must be greater than {Min} characters long."),
            > Max => throw new ArgumentOutOfRangeException(nameof(value), $"Value must be less than {Max} characters long."),
            _ => normalized
        };
    }

    public bool GrantScope(Scope scope)
        => _scopes.Add(scope);

    public bool RevokeScope(Scope scope)
        => _scopes.Remove(scope);
    
    public override string ToString() => Value;

    public bool Equals(Audience? other)
        => other is not null && Value == other.Value;

    public override bool Equals(object? obj)
        => obj is Audience other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Value);
}