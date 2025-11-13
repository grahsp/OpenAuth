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
    private Dictionary<string, string[]> _audiences = [];
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
        _secrets.Add(new SecretHash(secret));
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

    public ClientBuilder WithGrantType(string grantType = GrantTypes.ClientCredentials)
    {
        _grantTypes.Add(GrantType.Create(grantType));
        return this;
    }

    public ClientBuilder WithAudience(string audience, params string[] scopes)
    {
        _audiences.Add(audience, scopes);
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
        
        var audiences = _audiences.Select(a
            => new Audience(new AudienceName(a.Key),
                new ScopeCollection(a.Value.Select(s => new Scope(s)))
            ));

        var clientConfig = new ClientConfiguration(
            name, applicationType, audiences, _grantTypes, _redirectUris);
        
        var client = Client.Create(clientConfig, createdAt);
        
        foreach (var secret in _secrets)
            client.AddSecret(secret, createdAt);

        // if (_audiences.Count == 0)
        //     _audiences.Add("test-audience", ["read write"]);
        
        
        // client.SetAudiences(audiences, createdAt);
        
        // foreach (var redirectUri in _redirectUris)
        //     client.AddRedirectUri(redirectUri, createdAt);
        //
        // foreach (var grantType in _grantTypes)
        //     client.AddGrantType(grantType, createdAt);

        return client;
    }
}