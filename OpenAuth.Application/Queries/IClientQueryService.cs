using OpenAuth.Application.Dtos;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Queries;

public interface IClientQueryService
{
    Task<ClientInfo?> GetByIdAsync(ClientId id, CancellationToken ct = default);
    Task<ClientInfo?> GetByNameAsync(ClientName name, CancellationToken ct = default);
    Task<ClientDetails?> GetDetailsAsync(ClientId id, CancellationToken ct = default);
    Task<ClientTokenData?> GetTokenDataAsync(ClientId id, AudienceName audienceName, CancellationToken ct = default);
    Task<PagedResult<ClientInfo>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsAsync(ClientId id, CancellationToken ct = default);
    Task<bool> NameExistsAsync(ClientName name, CancellationToken ct = default);
}