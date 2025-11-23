using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace OpenAuth.Domain.Clients.ValueObjects;

public sealed record ScopeCollection : IReadOnlyCollection<Scope>
{
    private readonly HashSet<Scope> _items;
    
    public ScopeCollection(IEnumerable<Scope> scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        
        _items = scopes.ToHashSet();
    }

    public static ScopeCollection Parse(string spaceSeparatedScopes)
    {
        var scopes = spaceSeparatedScopes
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => new Scope(s))
            .ToArray();
        
        return new ScopeCollection(scopes);
    }

    public static bool TryParse(string? spaceSeparatedScopes, [NotNullWhen(true)] out ScopeCollection? scopes)
    {
        scopes = null;

        if (string.IsNullOrWhiteSpace(spaceSeparatedScopes))
            return false;
        
        try
        {
            scopes = Parse(spaceSeparatedScopes);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public int Count => _items.Count;
    public IEnumerator<Scope> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public bool Contains(Scope scope) => _items.Contains(scope);
    // public IEnumerable<Scope> Except(IEnumerable<Scope> scopes) => _items.Except(scopes);
    
    public override string ToString() => string.Join(' ', _items);

    public bool Equals(ScopeCollection? other)
        => other is not null && _items.SetEquals(other._items);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var scope in _items.OrderBy(s => s.Value))
            hash.Add(scope.GetHashCode());
        
        return hash.ToHashCode();
    }
}