namespace OpenAuth.Infrastructure.Persistence.Migrations;

public interface IDatabaseMigrator
{
	Task MigrateAsync(CancellationToken ct);
}
