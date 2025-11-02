using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class ClientIdentityBuilder
{
    private ClientId? _id;
    private ClientName? _name;

    public ClientIdentityBuilder WithId()
    {
        _id = ClientId.New();
        return this;
    }
    
    public ClientIdentityBuilder WithId(ClientId id)
    {
        _id = id;
        return this;
    }
    
    public ClientIdentityBuilder WithName(string name)
    {
        _name = new ClientName(name);
        return this;
    }
    
    public ClientIdentityBuilder WithName(ClientName name)
    {
        _name = name;
        return this;
    }

    public ClientIdentity Build()
    {
        var id = _id ?? ClientId.New();
        var name = _name ?? new ClientName("test-client");
        
        return new ClientIdentity(id, name);
    }
}