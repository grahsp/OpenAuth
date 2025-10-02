using System.Runtime.CompilerServices;
using OpenAuth.Domain.ValueObjects;

[assembly: InternalsVisibleTo("OpenAuth.Test.Common")]
[assembly: InternalsVisibleTo("OpenAuth.Infrastructure")]
namespace OpenAuth.Domain.Entities;

public sealed class Client
{
    public const int MaxSecrets = 3;
    
    private Client() { }

    private Client(ClientName name, DateTimeOffset utcNow)
    {
        Name = name;
        CreatedAt = UpdatedAt = utcNow;
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


    internal static Client Create(ClientName name, DateTimeOffset utcNow)
        => new(name, utcNow);

    public bool TryAddAudience(Audience audience, DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(audience);
        
        if (!_audiences.Add(audience))
            return false;
        
        Touch(utcNow);
        return true;
    }

    public bool TryRemoveAudience(Audience audience, DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(audience);
        
        if (!_audiences.Remove(audience))
            return false;
        
        Touch(utcNow);
        return true;
    }
    
    public void SetScopes(Audience audience, IEnumerable<Scope> scopes, DateTimeOffset utcNow)
    {
        if (!_audiences.TryGetValue(audience, out var aud))
            throw new InvalidOperationException("Audience not found.");

        foreach (var scope in aud.Scopes)
            aud.RevokeScope(scope);
        
        foreach (var scope in scopes)
            aud.GrantScope(scope);
        
        Touch(utcNow);
    }

    public void GrantScopes(Audience audience, IEnumerable<Scope> scopes, DateTimeOffset utcNow)
    {
        if (!_audiences.TryGetValue(audience, out var aud))
            throw new InvalidOperationException("Audience not found.");

        var updated = false;
        foreach (var scope in scopes)
            if (aud.GrantScope(scope))
                updated = true;

        if (updated)
            Touch(utcNow);
    }

    public void RevokeScopes(Audience audience, IEnumerable<Scope> scopes, DateTimeOffset utcNow)
    {
        if (!_audiences.TryGetValue(audience, out var aud))
            throw new InvalidOperationException("Audience not found.");

        var updated = false;
        foreach (var scope in scopes)
            if (aud.RevokeScope(scope))
                updated = true;
        
        if (updated)
            Touch(utcNow);
    }


    public void Rename(ClientName newName, DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(newName);
        
        if (newName == Name)
            return;
        
        Name = newName;
        Touch(utcNow);
    }

    public void SetTokenLifetime(TimeSpan value, DateTimeOffset utcNow)
    {
        if (value <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(value), "TokenLifetime must be positive.");
        
        if (value == TokenLifetime)
            return;
        
        TokenLifetime = value;
        Touch(utcNow);
    }
    
    
    public SecretId AddSecret(SecretHash hash, DateTimeOffset utcNow)
    {
        var secret = ClientSecret.Create(hash, utcNow, TimeSpan.FromDays(7));
        
        if (!secret.IsActive(utcNow))
            throw new InvalidOperationException("Cannot add expired secret.");
        
        if (Secrets.Any(x => x.Id == secret.Id))
            throw new InvalidOperationException("Secret with same ID exist.");
        
        if (Secrets.Count(x => x.IsActive(utcNow)) >= MaxSecrets)
            throw new InvalidOperationException($"Client cannot have more than { MaxSecrets } secrets.");
        
        Secrets.Add(secret);
        Touch(utcNow);

        return secret.Id;
    }
    
    public void RevokeSecret(SecretId secretId, DateTimeOffset utcNow)
    {
        var secret = Secrets.FirstOrDefault(x => x.Id == secretId)
            ?? throw new InvalidOperationException($"Secret { secretId } not found.");

        if (!secret.IsActive(utcNow))
            return;

        var activeSecrets = Secrets.Count(s => s.IsActive(utcNow));
        if (activeSecrets <= 1)
            throw new InvalidOperationException("Cannot revoke last secret.");
        
        secret.Revoke(utcNow);
        Touch(utcNow);
    }

    public IEnumerable<ClientSecret> ActiveSecrets(DateTimeOffset utcNow) =>
        Secrets.Where(x => x.IsActive(utcNow))
            .OrderByDescending(x => x.CreatedAt);

    public void Enable(DateTimeOffset utcNow)
    {
        if (Enabled)
            return;
        
        Enabled = true;
        Touch(utcNow);
    }

    public void Disable(DateTimeOffset utcNow)
    {
        if (!Enabled)
            return;
        
        Enabled = false;
        Touch(utcNow);
    }
    
    private void Touch(DateTimeOffset utcNow)
        => UpdatedAt = utcNow;
}