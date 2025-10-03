using OpenAuth.Application.Dtos;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Queries;

public interface IClientQueryService
{
    Task<ClientSummary?> GetByIdAsync(ClientId id, CancellationToken ct = default);
    Task<ClientSummary?> GetByNameAsync(ClientName name, CancellationToken ct = default);
    Task<ClientDetails?> GetDetailsAsync(ClientId id, CancellationToken ct = default);
    Task<PagedResult<ClientSummary>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsAsync(ClientId id, CancellationToken ct = default);
    Task<bool> NameExistsAsync(ClientName name, CancellationToken ct = default);
}