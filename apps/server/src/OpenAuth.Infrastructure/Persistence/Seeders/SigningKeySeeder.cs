using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.SigningKeys.Factories;
using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Infrastructure.Persistence.Seeders;

public class SigningKeySeeder(AppDbContext context, ISigningKeyFactory factory, TimeProvider time) : ISeeder
{
	private const int LifetimeInDays = 365;
	
	public async Task SeedAsync(CancellationToken ct)
	{
		var now = time.GetUtcNow();
		
		if (await context.SigningKeys.AnyAsync(x => x.ExpiresAt >= now, ct))
			return;
		
		var key = factory.Create(SigningAlgorithm.RS256, now, TimeSpan.FromDays(LifetimeInDays));
		context.SigningKeys.Add(key);
		
		await context.SaveChangesAsync(ct);
	}
}