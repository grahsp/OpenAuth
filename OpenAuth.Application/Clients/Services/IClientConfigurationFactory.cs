using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Services;

public interface IClientConfigurationFactory
{
    ClientConfiguration Create(RegisterClientRequest request);
}