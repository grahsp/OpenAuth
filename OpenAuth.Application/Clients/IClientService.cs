using OpenAuth.Application.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients;

public interface IClientService
{
    Task<ClientInfo> RegisterAsync(ClientName name, CancellationToken cancellationToken = default);
    Task<ClientInfo> RenameAsync(ClientId id, ClientName name, CancellationToken cancellationToken = default);
    Task DeleteAsync(ClientId id, CancellationToken cancellationToken = default);
    
    Task EnableAsync(ClientId id, CancellationToken cancellationToken = default);
    Task DisableAsync(ClientId id, CancellationToken cancellationToken = default);
}