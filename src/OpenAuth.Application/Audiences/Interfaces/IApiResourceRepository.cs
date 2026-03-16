using OpenAuth.Domain.ApiResources;
using OpenAuth.Domain.ApiResources.ValueObjects;

namespace OpenAuth.Application.Audiences.Interfaces;

public interface IApiResourceRepository
{
	Task<ApiResource?> GetByIdAsync(ApiResourceId id, CancellationToken ct = default);
	Task<ApiResource?> GetByAudienceAsync(AudienceIdentifier audience, CancellationToken ct = default);
	
	void Add(ApiResource apiResource);
	void Remove(ApiResource apiResource);
}