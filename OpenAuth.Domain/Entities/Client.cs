using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Domain.Entities;

public sealed class Client
{
    private Client() { }

    public Client(string name)
    {
        SetName(name, false);
        CreatedAt = UpdatedAt = DateTime.UtcNow;
    }
    

    // General
    public ClientId Id { get; init; } = ClientId.New();
    public string Name { get; private set; } = null!;
    public bool Enabled { get; private set; } = true;
    
    
    // Settings
    public TimeSpan TokenLifetime { get; private set; } = TimeSpan.FromMinutes(10);

    public List<ClientSecret> Secrets { get; private set; } = [];
    public List<SigningKey> SigningKeys { get; private set; } = [];

    
    private readonly HashSet<Audience> _audiences = [];
    public IReadOnlyCollection<Audience> Audiences => _audiences;
    public IReadOnlyCollection<Scope> GetAllowedScopes(Audience audience)
        => Audiences.FirstOrDefault(x => x.Value == audience.Value)?.Scopes ?? [];
    
    
    // Metadata
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }


    public bool TryAddAudience(Audience audience)
    {
        if (!_audiences.Add(audience))
            return false;
        
        Touch();
        return true;
    }

    public bool TryRemoveAudience(Audience audience)
    {
        if (!_audiences.Remove(audience))
            return false;
        
        Touch();
        return true;
    }
    
    public void SetScopes(Audience audience, params Scope[] scopes)
        => SetScopes(audience, (IEnumerable<Scope>)scopes);

    public void SetScopes(Audience audience, IEnumerable<Scope> scopes)
    {
        if (!_audiences.TryGetValue(audience, out var aud))
            throw new InvalidOperationException("Audience not found.");

        foreach (var scope in aud.Scopes)
            aud.RevokeScope(scope);
        
        foreach (var scope in scopes)
            aud.GrantScope(scope);
    }

    public void GrantScopes(Audience audience, params Scope[] scopes) =>
        GrantScopes(audience, (IEnumerable<Scope>)scopes);

    public void GrantScopes(Audience audience, IEnumerable<Scope> scopes)
    {
        if (!_audiences.TryGetValue(audience, out var aud))
            throw new InvalidOperationException("Audience not found.");

        var updated = false;
        foreach (var scope in scopes)
            if (aud.GrantScope(scope))
                updated = true;

        if (updated)
            Touch();
    }

    public void RevokeScopes(Audience audience, params Scope[] scopes) =>
        RevokeScopes(audience, (IEnumerable<Scope>)scopes);

    public void RevokeScopes(Audience audience, IEnumerable<Scope> scopes)
    {
        if (!_audiences.TryGetValue(audience, out var aud))
            throw new InvalidOperationException("Audience not found.");

        var updated = false;
        foreach (var scope in scopes)
            if (aud.RevokeScope(scope))
                updated = true;
        
        if (updated)
            Touch();
    }
    

    public void Rename(string name) => SetName(name, true);

    private void SetName(string name, bool updateTimeStamp)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        name = name.Trim();
        
        const int min = 3, max = 24;
        if (name.Length < min)
            throw new ArgumentOutOfRangeException(nameof(name), $"Client name cannot be shorter than {min} characters.");
        if (name.Length > max)
            throw new ArgumentOutOfRangeException(nameof(name), $"Client name cannot be longer than {max} characters.");

        if (name == Name)
            return;
        
        Name = name;
        if (updateTimeStamp)
            Touch();
    }
    
    public void SetTokenLifetime(TimeSpan value)
    {
        if (value <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(value), "TokenLifetime must be positive.");
        
        if (value == TokenLifetime)
            return;
        
        TokenLifetime = value;
        Touch();
    }

    public void AddSecret(ClientSecret secret)
    {
        ArgumentNullException.ThrowIfNull(secret);

        if (Secrets.Any(x => x.Id == secret.Id))
            throw new InvalidOperationException("Secret already exists under client.");
        
        Secrets.Add(secret);
        Touch();
    }

    public void AddSigningKey(SigningKey signingKey)
    {
        ArgumentNullException.ThrowIfNull(signingKey);

        if (SigningKeys.Any(x => x.KeyId == signingKey.KeyId))
            throw new InvalidOperationException("SigningKey already exists under client.");
        
        SigningKeys.Add(signingKey);
        Touch();
    }
    
    public void RevokeSecret(SecretId secretId)
    {
        var secret = Secrets.FirstOrDefault(x => x.Id == secretId);
        if (secret is null)
            return;
        
        secret.Revoke();
        Touch();
    }
    
    public void RevokeSigningKey(SigningKeyId keyId)
    {
        var signingKey = SigningKeys.FirstOrDefault(x => x.KeyId == keyId);
        if (signingKey is null)
            return;

        signingKey.Revoke();
        Touch();
    }

    public void RemoveSecret(SecretId secretId)
    {
        var secret = Secrets.FirstOrDefault(x => x.Id == secretId);
        if (secret is null)
            return;
        
        Secrets.Remove(secret);
        Touch();
    }
    
    public void RemoveSigningKey(SigningKeyId keyId)
    {
        var signingKey = SigningKeys.FirstOrDefault(x => x.KeyId == keyId);
        if (signingKey is null)
            return;

        SigningKeys.Remove(signingKey);
        Touch();
    }

    public IEnumerable<ClientSecret> ActiveSecrets() =>
        Secrets.Where(x => x.IsActive())
            .OrderByDescending(x => x.CreatedAt);

    public IEnumerable<SigningKey> ActiveSigningKeys() =>
        SigningKeys.Where(x => x.IsActive())
            .OrderByDescending(x => x.CreatedAt);

    public void Enable()
    {
        if (Enabled)
            return;
        
        Enabled = true;
        Touch();
    }

    public void Disable()
    {
        if (!Enabled)
            return;
        
        Enabled = false;
        Touch();
    }
    
    private void Touch() => UpdatedAt = DateTime.UtcNow;
}