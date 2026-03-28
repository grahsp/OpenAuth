using OpenAuth.Application.Abstractions;

namespace OpenAuth.Infrastructure.Persistence.Seeders;

public class SeederRunner(IUnitOfWork persistence, IEnumerable<ISeeder> seeders) : ISeederRunner
{
	public async Task RunAsync(CancellationToken ct)
	{
		foreach (var seeder in seeders)
			await seeder.SeedAsync(ct);

		await persistence.SaveChangesAsync(ct);
	}
}