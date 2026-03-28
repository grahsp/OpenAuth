using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Domain.ApiResources;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.ApiResources;

public class ApiResourceRepository(AppDbContext context) : IApiResourceRepository
{
	public Task<ApiResource?> GetByIdAsync(ApiResourceId id, CancellationToken ct = default) =>
		context.ApiResources.SingleOrDefaultAsync(x => x.Id == id, ct);
	
	public Task<ApiResource?> GetByAudienceAsync(AudienceIdentifier audience, CancellationToken ct = default) =>
		context.ApiResources.SingleOrDefaultAsync(x => x.Audience == audience, ct);
	
	public void Add(ApiResource apiResource) =>
		context.ApiResources.Add(apiResource);

	public void Remove(ApiResource apiResource) =>
		context.ApiResources.Remove(apiResource);
}