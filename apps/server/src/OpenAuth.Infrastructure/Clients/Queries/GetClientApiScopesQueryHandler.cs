using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.ApiResources;
using OpenAuth.Application.Clients.Queries.GetClientApiScopes;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.Clients.Queries;

public class GetClientApiScopesQueryHandler(AppDbContext context) : IQueryHandler<GetClientApiScopesQuery, ScopeCollection>
{
	public async Task<ScopeCollection> HandleAsync(GetClientApiScopesQuery query, CancellationToken ct)
	{
		var apiResourceId = await context.ApiResources
			.AsNoTracking()
			.Where(x => x.Audience == query.Audience)
			.Select(x => x.Id)
			.SingleOrDefaultAsync(ct);

		if (apiResourceId == default)
			throw new ApiResourceNotFoundException(query.Audience);

		var scopes = await context.Clients
			.Where(x => x.Id == query.ClientId)
			.SelectMany(x => x.Apis)
			.Where(api => api.ApiResourceId == apiResourceId)
			.Select(api => api.AllowedScopes)
			.SingleOrDefaultAsync(ct);
		
		return scopes ?? ScopeCollection.Empty;
	}
}