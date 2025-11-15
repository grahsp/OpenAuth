using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Domain.Clients;

public sealed class Client
{
    public ClientId Id { get; init; } = ClientId.New();
    public ClientName Name { get; private set; } = null!;
    
    public ClientApplicationType ApplicationType { get; init; }
    
    public const int MaxSecrets = 3;
    public List<Secret> Secrets { get; private set; } = [];
    
    
    // Authorization
    public bool IsPublic => !ApplicationType.AllowsClientSecrets;
    public bool IsConfidential => ApplicationType.AllowsClientSecrets;
    
    public bool RequirePkce { get; private set; }
    
    private HashSet<GrantType> _allowedGrantTypes = [];
    public IReadOnlyCollection<GrantType> AllowedGrantTypes => _allowedGrantTypes;

    private HashSet<RedirectUri> _redirectUris = [];
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

    [Obsolete]
    private Client(ClientName name, DateTimeOffset utcNow)
    {
        Name = name;
        CreatedAt = UpdatedAt = utcNow;
        
        // Temporary
        ApplicationType = new SinglePageClientApplicationType();
    }

    [Obsolete]
    internal static Client Create(ClientName name, DateTimeOffset utcNow)
        => new(name, utcNow);

    private Client(ClientConfiguration config, DateTimeOffset utcNow)
    {
        Name = config.Name;
        ApplicationType = config.ApplicationType;
        
        _allowedAudiences = config.AllowedAudiences.ToHashSet();
        _allowedGrantTypes = config.AllowedGrantTypes.ToHashSet();
        _redirectUris = config.RedirectUris.ToHashSet();
        
        CreatedAt = UpdatedAt = utcNow;
        
        ValidateInitialClient();
    }

    internal static Client Create(ClientConfiguration config, DateTimeOffset utcNow)
        => new(config, utcNow);

    private void ValidateInitialClient()
    {
        ApplicationType.ValidateAudiences(AllowedAudiences);
        ApplicationType.ValidateRedirectUris(RedirectUris);
        ApplicationType.ValidateGrantTypes(AllowedGrantTypes);
    }

    public void ValidateClient()
    {
        ValidateInitialClient();
        ApplicationType.ValidateSecrets(Secrets);
    }

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
    
    public void SetPkceRequirement(bool requirePkce, DateTimeOffset utcNow)
    {
        if (RequirePkce == requirePkce)
            return;

        RequirePkce = requirePkce;
        Touch(utcNow);
    }
    
    
    public void SetGrantTypes(IEnumerable<GrantType> grants, DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(grants);

        var items = grants.ToArray();
        
        if (items.Length == 0)
            throw new InvalidOperationException("Client must have at least one grant type.");
        
        if (items.Distinct().Count() != items.Length)
            throw new InvalidOperationException("Client cannot contain duplicate grant types.");

        if (_allowedGrantTypes.SetEquals(items))
            return;
        
        _allowedGrantTypes = items.ToHashSet();
        Touch(utcNow);
    }
    
    
    public void SetRedirectUris(IEnumerable<RedirectUri> uris, DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(uris);
        
        var items = uris.ToArray();
        
        if (items.Distinct().Count() != items.Length)
            throw new InvalidOperationException("Client cannot contain duplicate redirect URIs.");

        if (_redirectUris.SetEquals(items))
            return;
        
        _redirectUris = items.ToHashSet();
        Touch(utcNow);
    }
    
    
    public Audience GetAudience(AudienceName name)
        => _allowedAudiences.SingleOrDefault(a => a.Name == name) ??
           throw new InvalidOperationException($"Audience {name.Value} not found.");

    public void SetAudiences(IEnumerable<Audience> audiences, DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(audiences);
        
        var items = audiences.ToArray();
        
        if (items.Distinct().Count() != items.Length)
            throw new InvalidOperationException("Client cannot contain duplicate audience names.");

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