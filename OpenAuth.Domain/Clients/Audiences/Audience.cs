using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Clients.Audiences;

[Obsolete($"Use { nameof(NewAudience) } instead.")]
public sealed class Audience
{
    public AudienceId Id { get; private init; } = null!;
    public AudienceName Name { get; private init; }

    private readonly HashSet<Scope> _allowedScopes = [];
    public IReadOnlyCollection<Scope> AllowedScopes => _allowedScopes;
    
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset UpdatedAt { get; private set; }
    

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
        _allowedScopes.Clear();
        foreach (var scope in scopes)
            _allowedScopes.Add(scope);
        
        Touch(utcNow);
    }

    internal void GrantScopes(IEnumerable<Scope> scopes, DateTimeOffset utcNow)
    {
        foreach (var scope in scopes)
            _allowedScopes.Add(scope);
        
        Touch(utcNow);
    }

    internal void RevokeScope(IEnumerable<Scope> scopes, DateTimeOffset utcNow)
    {
        foreach (var scope in scopes)
            _allowedScopes.Remove(scope);
        
        Touch(utcNow);
    }
    
    private void Touch(DateTimeOffset utcNow)
        => UpdatedAt = utcNow;
}