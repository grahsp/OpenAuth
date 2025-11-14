using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Services;

public interface IClientService
{
    Task<RegisteredClientResponse> RegisterAsync(RegisterClientRequest request, CancellationToken ct = default);
    Task<ClientInfo> RenameAsync(ClientId id, ClientName name, CancellationToken ct = default);
    Task DeleteAsync(ClientId id, CancellationToken ct = default);
    
    Task<ClientDetails> SetAudiencesAsync(ClientId id, IEnumerable<Audience> audiences, CancellationToken ct = default);
    Task<ClientDetails> AddAudienceAsync(ClientId id, Audience audience, CancellationToken ct = default);
    Task<ClientDetails> RemoveAudienceAsync(ClientId id, AudienceName name, CancellationToken ct = default);
    
    Task EnableAsync(ClientId id, CancellationToken ct = default);
    Task DisableAsync(ClientId id, CancellationToken ct = default);
}