namespace OpenAuth.Domain.ValueObjects;

public sealed class Audience
{
    public AudienceId Id { get; private init; } = null!;
    public AudienceName Name { get; private init; } = null!;
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<Scope> Scopes => _scopes;
    private readonly HashSet<Scope> _scopes = [];

    private Audience() { }
    
    
    internal static Audience Create(AudienceName name, DateTimeOffset utcNow)
        => new()
        {
            Id = AudienceId.New(),
            Name = name,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

    internal void SetScopes(IEnumerable<Scope> scopes, DateTimeOffset utcNow)
    {
        _scopes.Clear();
        foreach (var scope in scopes)
            _scopes.Add(scope);
        
        Touch(utcNow);
    }

    internal void GrantScopes(IEnumerable<Scope> scopes, DateTimeOffset utcNow)
    {
        foreach (var scope in scopes)
            _scopes.Add(scope);
        
        Touch(utcNow);
    }

    internal void RevokeScope(IEnumerable<Scope> scopes, DateTimeOffset utcNow)
    {
        foreach (var scope in scopes)
            _scopes.Remove(scope);
        
        Touch(utcNow);
    }
    
    private void Touch(DateTimeOffset utcNow)
        => UpdatedAt = utcNow;
}