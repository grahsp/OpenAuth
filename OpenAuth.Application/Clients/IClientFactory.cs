using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Clients;

public interface IClientFactory
{
    Client Create(ClientName name);
}