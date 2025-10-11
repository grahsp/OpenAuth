using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class ClientBuilder
{
    private ClientName? _name;
    private DateTimeOffset? _createdAt;
    
    private List<SecretHash> _secrets = [];
    private Dictionary<string, string[]> _audiences = [];
    private List<RedirectUri> _redirectUris = [];
    private List<GrantType> _grantTypes = [];
    
    
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
        
        var client = Client.Create(name, createdAt);
        
        foreach (var secret in _secrets)
            client.AddSecret(secret, createdAt);

        foreach (var (audience, scopes) in _audiences)
        {
            var audienceName = new AudienceName(audience);
            client.AddAudience(audienceName, createdAt);
            client.GrantScopes(audienceName, scopes.Select(s => new Scope(s)), createdAt);
        }
        
        foreach (var redirectUri in _redirectUris)
            client.AddRedirectUri(redirectUri, createdAt);
        
        foreach (var grantType in _grantTypes)
            client.AddGrantType(grantType, createdAt);

        return client;
    }
}