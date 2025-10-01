using OpenAuth.Application.Clients;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.Clients;

public class ClientFactory : IClientFactory
{
    private readonly TimeProvider _time;
    
    public ClientFactory(TimeProvider time)
    {
        _time = time;
    }
    
    
    public Client Create(ClientName name)
        => Client.Create(name, _time.GetUtcNow());
}