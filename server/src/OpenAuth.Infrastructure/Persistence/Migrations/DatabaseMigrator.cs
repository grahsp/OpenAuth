using Microsoft.EntityFrameworkCore;

namespace OpenAuth.Infrastructure.Persistence.Migrations;

public class DatabaseMigrator(AppDbContext context) : IDatabaseMigrator
{
	public Task MigrateAsync(CancellationToken ct) =>
		context.Database.MigrateAsync(ct);
}
