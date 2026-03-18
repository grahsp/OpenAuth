using OpenAuth.Domain.ApiResources.ValueObjects;
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

	private readonly HashSet<ClientApiAccess> _apis = [];
	public IReadOnlyCollection<ClientApiAccess> Apis => _apis;
    
    
	// Authorization
	public bool IsPublic => !ApplicationType.AllowsClientSecrets;
	public bool IsConfidential => ApplicationType.AllowsClientSecrets;
    
	public bool RequiresRedirectUri => AllowedGrantTypes
		.Any(x => x.RequiresRedirectUri);
    
	private HashSet<GrantType> _allowedGrantTypes = [];
	public IReadOnlyCollection<GrantType> AllowedGrantTypes => _allowedGrantTypes;

	private HashSet<RedirectUri> _redirectUris = [];
	public IReadOnlyCollection<RedirectUri> RedirectUris => _redirectUris;
    
    
	// Token
	public TimeSpan TokenLifetime { get; private set; } = TimeSpan.FromMinutes(10);
    
    
	// Metadata
	public bool Enabled { get; private set; } = true;
	public DateTimeOffset CreatedAt { get; private set; }
	public DateTimeOffset UpdatedAt { get; private set; }
    
    
	private Client() { }

	private Client(
		ClientName name,
		ClientApplicationType applicationType,
		IEnumerable<GrantType> allowedGrantTypes,
		DateTimeOffset utcNow)
	{
		Name = name;
		ApplicationType = applicationType;
		_allowedGrantTypes = allowedGrantTypes.ToHashSet();
		CreatedAt = UpdatedAt = utcNow;
	}

	[Obsolete]
	internal static Client Create(
		ClientName name,
		ClientApplicationType applicationType,
		IEnumerable<GrantType> allowedGrantTypes,
		DateTimeOffset utcNow) =>
		new Client(name, applicationType, allowedGrantTypes, utcNow);

	public static Client CreateSpa(
		ClientName name,
		DateTimeOffset utcNow)
	{
		var type = ClientApplicationTypes.Spa;
		return new Client(name,
			type,
			type.DefaultGrantTypes,
			utcNow
		);
	}
	
	
	public static Client CreateWeb(
		ClientName name,
		DateTimeOffset utcNow)
	{
		var type = ClientApplicationTypes.Web;
		return new Client(name,
			type,
			type.DefaultGrantTypes,
			utcNow
		);
	}
	
	
	public static Client CreateM2M(
		ClientName name,
		ApiResourceId apiResourceId,
		ScopeCollection scopes,
		DateTimeOffset utcNow)
	{
		var type = ClientApplicationTypes.M2M;
		var client = new Client(name,
			type,
			type.DefaultGrantTypes,
			utcNow
		);
		
		client.GrantApiAccess(apiResourceId, scopes, utcNow);
		return client;
	}

	public void ValidateClient()
	{
		ValidateGrantTypes();
		ValidateSecrets();
	}

	private void ValidateGrantTypes()
	{
		var invalid = AllowedGrantTypes.Except(ApplicationType.AllowedGrants).ToList();
		if (invalid.Count <= 0)
			return;
        
		var invalidNames = string.Join(", ", invalid.Select(g => g.Value));
		throw new InvalidOperationException($"Grant type(s) {invalidNames} are not allowed for this client type.");
	}
    
	private void ValidateSecrets()
	{
		if (!ApplicationType.AllowsClientSecrets && Secrets.Count > 0)
			throw new InvalidOperationException("Client type does not allow client secrets.");
        
		if (ApplicationType.AllowsClientSecrets && Secrets.Count == 0)
			throw new InvalidOperationException("Client must have at least one secret.");
	}

    
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

    
	public void GrantApiAccess(ApiResourceId apiResourceId, ScopeCollection scopes, DateTimeOffset utcNow)
	{
		if (_apis.Any(a => a.ApiResourceId == apiResourceId))
			throw new InvalidOperationException("Client already has access to this API.");
        
		var access = ClientApiAccess.Create(apiResourceId, scopes);
		_apis.Add(access);
        
		Touch(utcNow);
	}

	public void RevokeApiAccess(ApiResourceId apiResourceId, DateTimeOffset utcNow)
	{
		var access = _apis.FirstOrDefault(a => a.ApiResourceId == apiResourceId);
		if (access is null)
			return;
        
		_apis.Remove(access);
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
	public Secret AddSecret(SecretHash hash, DateTimeOffset utcNow)
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

		return secret;
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