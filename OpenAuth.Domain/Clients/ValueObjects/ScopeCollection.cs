namespace OpenAuth.Domain.Clients.ValueObjects;

public record ScopeCollection
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
}