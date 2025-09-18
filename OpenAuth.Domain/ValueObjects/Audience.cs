namespace OpenAuth.Domain.ValueObjects;

public sealed class Audience
{
    public const int Min = 3, Max = 16;
    
    public string Value { get; private set; }
    
    public IReadOnlyCollection<Scope> Scopes => _scopes;
    private readonly HashSet<Scope> _scopes = [];

    public Audience()
    { }
    
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

    public bool GrantScope(Scope scope)
        => _scopes.Add(scope);

    public bool RevokeScope(Scope scope)
        => _scopes.Remove(scope);
    
    public override string ToString() => Value;
}