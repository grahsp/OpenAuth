using System.Runtime.CompilerServices;
using OpenAuth.Domain.ValueObjects;

[assembly: InternalsVisibleTo("OpenAuth.Test.Common")]
[assembly: InternalsVisibleTo("OpenAuth.Test.Unit")]
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
    
    
    // Metadata
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }


    internal static Client Create(ClientName name, DateTimeOffset utcNow)
        => new(name, utcNow);

    
    public Audience GetAudience(AudienceName name)
        => _audiences.SingleOrDefault(a => a.Name == name) ??
           throw new InvalidOperationException($"Audience {name.Value} not found.");
    
    public Audience AddAudience(AudienceName name, DateTimeOffset utcNow)
    {
        if (_audiences.Any(a => a.Name == name))
            throw new InvalidOperationException($"Audience {name.Value} already exists.");

        var audience = Audience.Create(name, utcNow);
        _audiences.Add(audience);
        
        Touch(utcNow);
        return audience;
    }

    public void RemoveAudience(AudienceName name, DateTimeOffset utcNow)
    {
        var audience = GetAudience(name);
        _audiences.Remove(audience);
        
        Touch(utcNow);
    }

    
    public Audience SetScopes(AudienceName name, IEnumerable<Scope> scopes, DateTimeOffset utcNow)
    {
        var audience = GetAudience(name);
        audience.SetScopes(scopes, utcNow);
        
        Touch(utcNow);
        return audience;
    }

    public Audience GrantScopes(AudienceName name, IEnumerable<Scope> scopes, DateTimeOffset utcNow)
    {
        var audience = GetAudience(name);
        audience.GrantScopes(scopes, utcNow);
        
        Touch(utcNow);
        return audience;
    }

    public Audience RevokeScopes(AudienceName name, IEnumerable<Scope> scopes, DateTimeOffset utcNow)
    {
        var audience = GetAudience(name);
        audience.RevokeScope(scopes, utcNow);
        
        Touch(utcNow);
        return audience;
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