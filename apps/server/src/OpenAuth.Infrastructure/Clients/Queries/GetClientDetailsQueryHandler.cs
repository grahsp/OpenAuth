using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Queries.GetClientDetails;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.Clients.Queries;

public class GetClientDetailsQueryHandler(AppDbContext context)
	: IQueryHandler<GetClientDetailsQuery, ClientDetails?>
{
	public async Task<ClientDetails?> HandleAsync(GetClientDetailsQuery query, CancellationToken ct)
	{
		var data = await context.Clients
			.AsNoTracking()
			.Where(x => x.Id == query.Id)
			.Select(x => new
			{
				x.Id,
				x.Name,
				x.ApplicationType,
				x.RedirectUris,
				x.TokenLifetime,
				x.AllowedGrantTypes
			})
			.SingleOrDefaultAsync(ct);

		if (data is null)
			return null;

		var availableGrantTypes = data.ApplicationType.AllowedGrants
			.Select(g => g.Value)
			.ToList();

		return new ClientDetails(
			data.Id.Value,
			data.Name.Value,
			data.ApplicationType.Name,
			data.RedirectUris.Select(r => r.Value).ToList(),
			data.TokenLifetime.TotalSeconds,
			data.AllowedGrantTypes.Select(g => g.Value).ToList(),
			availableGrantTypes
		);
	}
}