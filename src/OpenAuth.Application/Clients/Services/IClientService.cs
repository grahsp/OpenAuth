using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Services;

public interface IClientService
{
    Task<CreateClientResult> CreateClientAsync(CreateClientCommand command, CancellationToken ct = default);
    Task<ClientInfo> RenameAsync(ClientId id, ClientName name, CancellationToken ct = default);
    Task DeleteAsync(ClientId id, CancellationToken ct = default);
    
    Task<ClientDetailsResult> SetGrantTypesAsync(ClientId id, IEnumerable<GrantType> grantTypes, CancellationToken ct = default);
    Task<ClientDetailsResult> AddGrantTypeAsync(ClientId id, GrantType grantType, CancellationToken ct = default);
    Task<ClientDetailsResult> RemoveGrantTypeAsync(ClientId id, GrantType grantType, CancellationToken ct = default);
    
    Task<ClientDetailsResult> SetRedirectUrisAsync(ClientId id, IEnumerable<RedirectUri> redirectUris, CancellationToken ct = default);
    Task<ClientDetailsResult> AddRedirectUriAsync(ClientId id, RedirectUri redirectUri, CancellationToken ct = default);
    Task<ClientDetailsResult> RemoveRedirectUriAsync(ClientId id, RedirectUri redirectUri, CancellationToken ct = default);
    
    Task<ClientDetailsResult> SetAudiencesAsync(ClientId id, IEnumerable<Audience> audiences, CancellationToken ct = default);
    Task<ClientDetailsResult> AddAudienceAsync(ClientId id, Audience audience, CancellationToken ct = default);
    Task<ClientDetailsResult> RemoveAudienceAsync(ClientId id, AudienceName name, CancellationToken ct = default);
    
    Task EnableAsync(ClientId id, CancellationToken ct = default);
    Task DisableAsync(ClientId id, CancellationToken ct = default);
}