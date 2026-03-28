using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OpenAuth.Infrastructure.Persistence.Seeders;

public static class SeederExtensions
{
	public static async Task SeedAsync(this WebApplication app, CancellationToken ct = default)
	{
		var scope = app.Services.CreateAsyncScope();
		var runner = scope.ServiceProvider.GetRequiredService<ISeederRunner>();
		
		await runner.RunAsync(ct);
	}
}