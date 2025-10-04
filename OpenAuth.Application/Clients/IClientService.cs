using OpenAuth.Application.Dtos;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Clients;

public interface IClientService
{
    Task<ClientInfo> RegisterAsync(ClientName name, CancellationToken cancellationToken = default);
    Task<ClientInfo> RenameAsync(ClientId id, ClientName name, CancellationToken cancellationToken = default);
    Task DeleteAsync(ClientId id, CancellationToken cancellationToken = default);
    
    Task EnableAsync(ClientId id, CancellationToken cancellationToken = default);
    Task DisableAsync(ClientId id, CancellationToken cancellationToken = default);
    
    Task<Client?> TryAddAudienceAsync(ClientId id, Audience audience, CancellationToken cancellationToken = default);
    Task<Client?> TryRemoveAudienceAsync(ClientId id, Audience audience, CancellationToken cancellationToken = default);
    Task<Client> SetScopesAsync(ClientId id, Audience audience, IEnumerable<Scope> scopes, CancellationToken cancellationToken = default);
    Task<Client> GrantScopesAsync(ClientId id, Audience audience, IEnumerable<Scope> scopes, CancellationToken cancellationToken = default);
    Task<Client> RevokeScopesAsync(ClientId id, Audience audience, IEnumerable<Scope> scopes, CancellationToken cancellationToken = default);

}