using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients;

public interface IClientFactory
{
    Client Create(ClientName name);
}