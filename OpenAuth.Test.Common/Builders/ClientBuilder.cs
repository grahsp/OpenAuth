using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class ClientBuilder
{
    private ClientName _name = new("Client");
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    
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

    public ClientBuilder CreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }
    
    public Client Build()
        => Client.Create(_name, _createdAt);
}