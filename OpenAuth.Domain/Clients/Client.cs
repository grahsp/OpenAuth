using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Domain.Clients;

public sealed class Client
{
    public ClientId Id { get; init; } = ClientId.New();
    public ClientName Name { get; private set; } = null!;
    
    public const int MaxSecrets = 3;
    public List<Secret> Secrets { get; private set; } = [];
    
    
    // Authorization
    public bool IsPublic { get; private set; }
    public bool RequirePkce { get; private set; }
    
    private readonly HashSet<GrantType> _allowedGrantTypes = [];
    public IReadOnlyCollection<GrantType> AllowedGrantTypes => _allowedGrantTypes;

    private readonly HashSet<RedirectUri> _redirectUris = [];
    public IReadOnlyCollection<RedirectUri> RedirectUris => _redirectUris;
    
    private HashSet<Audience> _allowedAudiences = [];
    public IReadOnlyCollection<Audience> AllowedAudiences => _allowedAudiences;
    
    
    // Token
    public TimeSpan TokenLifetime { get; private set; } = TimeSpan.FromMinutes(10);
    
    
    // Metadata
    public bool Enabled { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    
    
    private Client() { }

    private Client(ClientName name, DateTimeOffset utcNow)
    {
        Name = name;
        CreatedAt = UpdatedAt = utcNow;
    }

    internal static Client Create(ClientName name, DateTimeOffset utcNow)
        => new(name, utcNow);

    // Client
    public void Rename(ClientName newName, DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(newName);
        
        if (newName == Name)
            return;
        
        Name = newName;
        Touch(utcNow);
    }

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
    
    
    public void SetPublic(bool isPublic, DateTimeOffset utcNow)
    {
        if (IsPublic == isPublic)
            return;
        
        IsPublic = isPublic;
        Touch(utcNow);
    }

    public void SetPkceRequirement(bool requirePkce, DateTimeOffset utcNow)
    {
        if (RequirePkce == requirePkce)
            return;

        RequirePkce = requirePkce;
        Touch(utcNow);
    }
    
    
    // GrantTypes
    public void AddGrantType(GrantType grantType, DateTimeOffset utcNow)
    {
        if (_allowedGrantTypes.Add(grantType))
            Touch(utcNow);
    }

    public void RemoveGrantType(GrantType grantType, DateTimeOffset utcNow)
    {
        if (_allowedGrantTypes.Remove(grantType))
            Touch(utcNow);
    }
    
    
    // RedirectUris
    public void AddRedirectUri(RedirectUri uri, DateTimeOffset utcNow)
    {
        if (_redirectUris.Add(uri))
            Touch(utcNow);
    }

    public void RemoveRedirectUri(RedirectUri uri, DateTimeOffset utcNow)
    {
        if (_redirectUris.Remove(uri))
            Touch(utcNow);
    }
    
    
    // Audiences
    public Audience GetAudience(AudienceName name)
        => _allowedAudiences.SingleOrDefault(a => a.Name == name) ??
           throw new InvalidOperationException($"Audience {name.Value} not found.");

    public void SetAudiences(IEnumerable<Audience> audiences, DateTimeOffset utcNow)
    {
        var items = audiences.ToArray();
        
        if (items.Select(a => a.Name.NormalizedValue).Distinct().Count() != items.Length)
            throw new InvalidOperationException("Client cannot contain duplicate audience names.");

        if (items.Length == 0)
            throw new InvalidOperationException("Client must have at least one audience.");

        if (_allowedAudiences.SetEquals(items))
            return;

        _allowedAudiences = items.ToHashSet();
        Touch(utcNow);
    }


    // Token
    public void SetTokenLifetime(TimeSpan value, DateTimeOffset utcNow)
    {
        if (value <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(value), "TokenLifetime must be positive.");
        
        if (value == TokenLifetime)
            return;
        
        TokenLifetime = value;
        Touch(utcNow);
    }
    
    
    // Secrets
    public SecretId AddSecret(SecretHash hash, DateTimeOffset utcNow)
    {
        var secret = Secret.Create(Id, hash, utcNow, TimeSpan.FromDays(7));
        
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

    public IEnumerable<Secret> ActiveSecrets(DateTimeOffset utcNow) =>
        Secrets.Where(x => x.IsActive(utcNow))
            .OrderByDescending(x => x.CreatedAt);
    
    
    private void Touch(DateTimeOffset utcNow)
        => UpdatedAt = utcNow;
}