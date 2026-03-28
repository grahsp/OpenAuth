namespace OpenAuth.Infrastructure.Persistence.Seeders;

public class SeederRunner(IEnumerable<ISeeder> seeders) : ISeederRunner
{
	public async Task RunAsync(CancellationToken ct)
	{
		foreach (var seeder in seeders)
			await seeder.SeedAsync(ct);
	}
}