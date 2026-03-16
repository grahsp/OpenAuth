using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.Modules;

public static class PersistenceModule
{
	public static IServiceCollection AddPersistenceModule(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<AppDbContext>(opts =>
		{
			opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
		});

		return services;
	}
}