using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Services;

public interface IClientService
{
    Task<CreateClientResult> CreateClientAsync(CreateClientCommand command, CancellationToken ct = default);
    Task<ClientInfo> RenameAsync(ClientId id, ClientName name, CancellationToken ct = default);
    Task DeleteAsync(ClientId id, CancellationToken ct = default);
    
    Task<ClientDetails> SetGrantTypesAsync(ClientId id, IEnumerable<GrantType> grantTypes, CancellationToken ct = default);
    Task<ClientDetails> AddGrantTypeAsync(ClientId id, GrantType grantType, CancellationToken ct = default);
    Task<ClientDetails> RemoveGrantTypeAsync(ClientId id, GrantType grantType, CancellationToken ct = default);
    
    Task<ClientDetails> SetRedirectUrisAsync(ClientId id, IEnumerable<RedirectUri> redirectUris, CancellationToken ct = default);
    Task<ClientDetails> AddRedirectUriAsync(ClientId id, RedirectUri redirectUri, CancellationToken ct = default);
    Task<ClientDetails> RemoveRedirectUriAsync(ClientId id, RedirectUri redirectUri, CancellationToken ct = default);
    
    Task<ClientDetails> SetAudiencesAsync(ClientId id, IEnumerable<Audience> audiences, CancellationToken ct = default);
    Task<ClientDetails> AddAudienceAsync(ClientId id, Audience audience, CancellationToken ct = default);
    Task<ClientDetails> RemoveAudienceAsync(ClientId id, AudienceName name, CancellationToken ct = default);
    
    Task EnableAsync(ClientId id, CancellationToken ct = default);
    Task DisableAsync(ClientId id, CancellationToken ct = default);
}