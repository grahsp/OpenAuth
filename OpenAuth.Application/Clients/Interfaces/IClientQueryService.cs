using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Shared.Models;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Interfaces;

public interface IClientQueryService
{
    Task<ClientInfo?> GetByIdAsync(ClientId id, CancellationToken ct = default);
    Task<ClientInfo?> GetByNameAsync(ClientName name, CancellationToken ct = default);
    Task<ClientDetails?> GetDetailsAsync(ClientId id, CancellationToken ct = default);
    Task<ClientTokenData?> GetTokenDataAsync(ClientId id, CancellationToken ct = default);
    Task<ClientAuthorizationData?> GetAuthorizationDataAsync(ClientId id, CancellationToken ct = default);
    Task<PagedResult<ClientInfo>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsAsync(ClientId id, CancellationToken ct = default);
    Task<bool> NameExistsAsync(ClientName name, CancellationToken ct = default);
}