using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class ClientBuilder
{
    private ClientName _name = new ClientName("Client");
    private TimeProvider _timeProvider = TimeProvider.System;
    
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

    public ClientBuilder WithTimeProvider(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        return this;
    }
    
    public Client Build()
    => Client.Create(_name, _timeProvider);
}