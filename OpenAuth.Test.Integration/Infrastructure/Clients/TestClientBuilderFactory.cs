using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Integration.Infrastructure.Clients;

public class TestClientBuilderFactory
{
    private readonly IClientService _service;
    
    public TestClientBuilderFactory(IClientService service)
    {
        _service = service;
    }
    
    private CreateClientRequest CreateDefaultRequest(ClientApplicationType type, string name)
        => new(type, ClientName.Create(name), [], []);

    public TestClientBuilder Spa(string name = "client")
        => new(_service, CreateDefaultRequest(ClientApplicationTypes.Spa, name));
    
    public TestClientBuilder M2M(string name = "client")
        => new(_service, CreateDefaultRequest(ClientApplicationTypes.M2M, name));
    
    public TestClientBuilder Web(string name = "client")
        => new(_service, CreateDefaultRequest(ClientApplicationTypes.Web, name));
}
