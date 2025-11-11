using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Clients.Factories;

public interface IClientFactory
{
    Client Create(ClientName name);
    Client Create(ClientConfiguration config);
}