using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Api;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Test.Common.Hosting;

public class TestHostFactory(TestHostSettings settings) : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration((_, config) =>
		{
			config.AddUserSecrets<Program>();
			foreach (var action in settings.ConfigOverrides)
				action(config);
		});

		builder.ConfigureServices(services =>
		{
			if (settings.DbConfigure is not null)
			{
				services.RemoveAll<AppDbContext>();
				services.RemoveAll<DbContextOptions<AppDbContext>>();
				services.AddDbContext<AppDbContext>(settings.DbConfigure);
			}

			// Integration testing defaults
			var time = new FakeTimeProvider();
			services.RemoveAll<TimeProvider>();
			services.AddSingleton<TimeProvider>(time);
			services.AddSingleton<FakeTimeProvider>(time);
			
			// User overrides
			foreach (var action in settings.ServiceOverrides)
				action(services);
		});
	}

	public static TestHost Create(Action<TestHostBuilder>? configure = null)
	{
		var builder = new TestHostBuilder();
		configure?.Invoke(builder);

		var settings = builder.Build();
		var factory = new TestHostFactory(settings);
		return new TestHost(factory);
	}
}
