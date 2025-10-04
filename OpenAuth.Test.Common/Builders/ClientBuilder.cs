using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class ClientBuilder
{
    private ClientName? _name;
    private DateTimeOffset? _createdAt;
    
    private List<SecretHash> _secrets = [];
    private List<Audience> _audiences = [];
    
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
    
    public ClientBuilder WithSecret(string secret)
    {
        _secrets.Add(new SecretHash(secret));
        return this;
    }

    public ClientBuilder WithSecret(SecretHash secret)
    {
        _secrets.Add(secret);
        return this;
    }

    public ClientBuilder WithAudience(string audience, params string[] scopes)
    {
        var aud = new Audience(audience);
        foreach (var s in scopes)
        {
            var scope = new Scope(s);
            aud.GrantScope(scope);
        }
        
        _audiences.Add(aud);
        
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

        foreach (var audience in _audiences)
            client.TryAddAudience(audience, createdAt);

        return client;
    }
}