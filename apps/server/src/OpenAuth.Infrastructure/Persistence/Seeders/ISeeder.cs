namespace OpenAuth.Infrastructure.Persistence.Seeders;

public interface ISeeder
{
	Task SeedAsync(CancellationToken ct);
}