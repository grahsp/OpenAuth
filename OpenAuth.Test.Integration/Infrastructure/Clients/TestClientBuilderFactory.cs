using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Integration.Infrastructure.Clients;

public class TestClientBuilderFactory
{
    private readonly IServiceProvider _services;
    
    public TestClientBuilderFactory(IServiceProvider services)
    {
        _services = services;
    }
    
    private CreateClientRequest CreateDefaultRequest(ClientApplicationType type)
        => new(type, ClientName.Create("test-client"), [], []);

    public TestClientBuilder Spa()
        => new(_services, CreateDefaultRequest(ClientApplicationTypes.Spa));
    
    public TestClientBuilder M2M()
        => new(_services, CreateDefaultRequest(ClientApplicationTypes.M2M));
    
    public TestClientBuilder Web()
        => new(_services, CreateDefaultRequest(ClientApplicationTypes.Web));
}
