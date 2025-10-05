using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class ClientBuilder
{
    private ClientName? _name;
    private DateTimeOffset? _createdAt;
    
    private List<SecretHash> _secrets = [];
    
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

        return client;
    }
}