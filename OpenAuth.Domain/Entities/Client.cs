using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Domain.Entities;

public sealed class Client
{
    private Client() { }

    public Client(ClientName name, DateTimeOffset now)
    {
        Name = name;
        CreatedAt = UpdatedAt = now;
    }
    

    // General
    public ClientId Id { get; init; } = ClientId.New();
    public ClientName Name { get; private set; } = null!;
    public bool Enabled { get; private set; } = true;
    
    
    // Settings
    public TimeSpan TokenLifetime { get; private set; } = TimeSpan.FromMinutes(10);

    public List<ClientSecret> Secrets { get; private set; } = [];

    
    private readonly HashSet<Audience> _audiences = [];
    public IReadOnlyCollection<Audience> Audiences => _audiences;
    public IReadOnlyCollection<Scope> GetAllowedScopes(Audience audience)
        => Audiences.FirstOrDefault(x => x.Value == audience.Value)?.Scopes ?? [];
    
    
    // Metadata
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }


    public bool TryAddAudience(Audience audience, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(audience);
        
        if (!_audiences.Add(audience))
            return false;
        
        Touch(now);
        return true;
    }

    public bool TryRemoveAudience(Audience audience, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(audience);
        
        if (!_audiences.Remove(audience))
            return false;
        
        Touch(now);
        return true;
    }
    
    public void SetScopes(Audience audience, IEnumerable<Scope> scopes, DateTimeOffset now)
    {
        if (!_audiences.TryGetValue(audience, out var aud))
            throw new InvalidOperationException("Audience not found.");

        foreach (var scope in aud.Scopes)
            aud.RevokeScope(scope);
        
        foreach (var scope in scopes)
            aud.GrantScope(scope);
        
        Touch(now);
    }

    public void GrantScopes(Audience audience, IEnumerable<Scope> scopes, DateTimeOffset now)
    {
        if (!_audiences.TryGetValue(audience, out var aud))
            throw new InvalidOperationException("Audience not found.");

        var updated = false;
        foreach (var scope in scopes)
            if (aud.GrantScope(scope))
                updated = true;

        if (updated)
            Touch(now);
    }

    public void RevokeScopes(Audience audience, IEnumerable<Scope> scopes, DateTimeOffset now)
    {
        if (!_audiences.TryGetValue(audience, out var aud))
            throw new InvalidOperationException("Audience not found.");

        var updated = false;
        foreach (var scope in scopes)
            if (aud.RevokeScope(scope))
                updated = true;
        
        if (updated)
            Touch(now);
    }


    public void Rename(ClientName newName, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(newName);
        
        if (newName == Name)
            return;
        
        Name = newName;
        Touch(now);
    }

    public void SetTokenLifetime(TimeSpan value, DateTimeOffset now)
    {
        if (value <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(value), "TokenLifetime must be positive.");
        
        if (value == TokenLifetime)
            return;
        
        TokenLifetime = value;
        Touch(now);
    }

    public void AddSecret(ClientSecret secret, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(secret);

        if (Secrets.Any(x => x.Id == secret.Id))
            throw new InvalidOperationException("Secret already exists under client.");
        
        Secrets.Add(secret);
        Touch(now);
    }
    
    public void RevokeSecret(SecretId secretId, DateTimeOffset now)
    {
        var secret = Secrets.FirstOrDefault(x => x.Id == secretId);
        if (secret is null)
            return;
        
        secret.Revoke();
        Touch(now);
    }

    public void RemoveSecret(SecretId secretId, DateTimeOffset now)
    {
        var secret = Secrets.FirstOrDefault(x => x.Id == secretId);
        if (secret is null)
            return;
        
        Secrets.Remove(secret);
        Touch(now);
    }

    public IEnumerable<ClientSecret> ActiveSecrets() =>
        Secrets.Where(x => x.IsActive())
            .OrderByDescending(x => x.CreatedAt);

    public void Enable(DateTimeOffset now)
    {
        if (Enabled)
            return;
        
        Enabled = true;
        Touch(now);
    }

    public void Disable(DateTimeOffset now)
    {
        if (!Enabled)
            return;
        
        Enabled = false;
        Touch(now);
    }
    
    private void Touch(DateTimeOffset now)
        => UpdatedAt = now;
}