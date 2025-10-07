using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Factories;

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