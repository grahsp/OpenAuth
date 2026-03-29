using OpenAuth.Infrastructure.Persistence.Migrations;

namespace OpenAuth.Server.Extensions;

public static class DatabaseInitializationExtensions
{
	public static async Task InitializeDatabaseAsync(this WebApplication app, CancellationToken ct)
	{
		if (!app.Configuration.GetValue<bool>("Database:Initialize"))
			return;
		
		using var scope = app.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>();
		await db.MigrateAsync(ct);
	}
}