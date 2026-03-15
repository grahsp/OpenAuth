using OpenAuth.Domain.Apis;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public class ClientBuilder
{
    private string _name = DefaultValues.ClientName;
    private ClientApplicationType? _applicationType;
    private DateTimeOffset? _createdAt;
    
    private List<string> _secrets = [];
    private List<(ApiResource, ScopeCollection)> _apiAccess = [];
    private List<string> _redirectUris = [];
    private List<GrantType> _grantTypes = [];


    public ClientBuilder WithApplicationType(ClientApplicationType applicationType)
    {
        _applicationType = applicationType;
        return this;
    }
    
    public ClientBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public ClientBuilder WithName(ClientName name)
    {
        _name = name.Value;
        return this;
    }
    
    public ClientBuilder WithSecret(string secret = "$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy")
    {
        _secrets.Add(secret);
        return this;
    }

    public ClientBuilder WithRedirectUri(string uri)
    {
        _redirectUris.Add(uri);
        return this;
    }
    
    public ClientBuilder WithGrantType(GrantType grantType)
    {
        _grantTypes.Add(grantType);
        return this;
    }
    
    public ClientBuilder WithApi(ApiResource api, string? scopes = null)
    {
        var collection = ScopeCollection.Parse(scopes ?? "read write");
        
        _apiAccess.Add((api, collection));
        return this;
    }

    public ClientBuilder CreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public Client Build()
    {
        var name = ClientName.Create(_name);
        var createdAt = _createdAt ?? DateTimeOffset.UtcNow;
        
        var applicationType = _applicationType ?? ClientApplicationTypes.Spa;

        if (_grantTypes.Count == 0)
            _grantTypes = applicationType.DefaultGrantTypes.ToList();

        if (_grantTypes.Any(g => g.RequiresRedirectUri) && _redirectUris.Count == 0)
            WithRedirectUri(DefaultValues.RedirectUri);

        if (applicationType.AllowsClientSecrets && _secrets.Count == 0)
            WithSecret();
        
        var redirectUris = _redirectUris.Select(RedirectUri.Create).ToList();
        
        var client = Client.Create(
            name,
            applicationType,
            applicationType.DefaultGrantTypes,
            redirectUris,
            createdAt);
        
        foreach (var secret in _secrets)
            client.AddSecret(SecretHash.FromHash(secret), createdAt);
        
        foreach (var (api, scopes) in _apiAccess)
            client.GrantApiAccess(api.Id, scopes, createdAt);

        return client;
    }
}