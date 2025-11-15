using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Test.Common.Builders;

public class ClientBuilder
{
    private ClientName? _name;
    private ClientApplicationType? _applicationType;
    private DateTimeOffset? _createdAt;
    
    private List<SecretHash> _secrets = [];
    private List<Audience> _audiences = [];
    private List<RedirectUri> _redirectUris = [];
    private List<GrantType> _grantTypes = [];


    public ClientBuilder WithApplicationType(ClientApplicationType applicationType)
    {
        _applicationType = applicationType;
        return this;
    }
    
    public ClientBuilder WithName(string name)
    {
        _name = new ClientName(name);
        return this;
    }
    
    public ClientBuilder WithName(ClientName name)
    {
        _name = name;
        return this;
    }
    
    public ClientBuilder WithSecret(string secret = "$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy")
    {
        _secrets.Add(SecretHash.FromHash(secret));
        return this;
    }

    public ClientBuilder WithSecret(SecretHash secret)
    {
        _secrets.Add(secret);
        return this;
    }

    public ClientBuilder WithRedirectUri(string uri = "http://example.com/callback")
    {
        _redirectUris.Add(RedirectUri.Create(uri));
        return this;
    }
    
    
    public ClientBuilder WithGrantType(GrantType grantType)
    {
        _grantTypes.Add(grantType);
        return this;
    }

    public ClientBuilder WithGrantType(string grantType = GrantTypes.ClientCredentials)
    {
        _grantTypes.Add(GrantType.Create(grantType));
        return this;
    }

    public ClientBuilder WithAudience(Audience audience)
    {
        _audiences.Add(audience);
        return this;
    }

    public ClientBuilder WithAudience(string audienceName, params string[] scopes)
    {
        var scopeCollection = new ScopeCollection(scopes.Select(s => new Scope(s)));
        var audience = new Audience(AudienceName.Create(audienceName), scopeCollection);
        
        _audiences.Add(audience);
        return this;
    }

    public ClientBuilder CreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public Client Build()
    {
        var name = _name ?? new ClientName("Client");
        var createdAt = _createdAt ?? DateTimeOffset.UtcNow;
        
        var applicationType = _applicationType ?? ClientApplicationTypes.Spa;

        if (applicationType.RequiresPermissions && _audiences.Count == 0)
            WithAudience("test-audience", "read", "write");
        
        if (applicationType.RequiresRedirectUris && _redirectUris.Count == 0)
            WithRedirectUri();

        if (applicationType.AllowsClientSecrets && _secrets.Count == 0)
            WithSecret();
        

        var clientConfig = new ClientConfiguration(
            name, applicationType, _audiences, _grantTypes, _redirectUris);
        
        var client = Client.Create(clientConfig, createdAt);
        
        foreach (var secret in _secrets)
            client.AddSecret(secret, createdAt);

        return client;
    }
}