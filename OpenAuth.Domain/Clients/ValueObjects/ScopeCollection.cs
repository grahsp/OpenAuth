namespace OpenAuth.Domain.Clients.ValueObjects;

public sealed record ScopeCollection : IEquatable<ScopeCollection>
{
    private readonly HashSet<string> _scopes;
    public IReadOnlyCollection<string> Scopes => _scopes;
    
    public ScopeCollection(IEnumerable<string> scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        
        _scopes = scopes
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToHashSet();
    }

    public static ScopeCollection Parse(string spaceSeparatedScopes)
    {
        string[] scopes = [];
        if (!string.IsNullOrWhiteSpace(spaceSeparatedScopes))
            scopes = spaceSeparatedScopes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        return new ScopeCollection(scopes);
    }

    public bool Contains(string scope) => _scopes.Contains(scope);
    
    public override string ToString() => string.Join(' ', _scopes);

    public bool Equals(ScopeCollection? other)
    {
        if (other is null)
            return false;

        return _scopes.SetEquals(other._scopes);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var scope in _scopes.Order())
            hash.Add(scope, StringComparer.Ordinal);
        
        return hash.ToHashCode();
    }
}