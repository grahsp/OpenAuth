using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Factories;

public interface IClientFactory
{
    Client Create(ClientName name);
}