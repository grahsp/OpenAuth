using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Abstractions;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Infrastructure.Persistence.Seeders;

namespace OpenAuth.Infrastructure.Modules;

public static class PersistenceModule
{
	public static IServiceCollection AddPersistenceModule(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<AppDbContext>(opts =>
		{
			opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
				sql => sql.EnableRetryOnFailure());
		});

		services.AddScoped<IUnitOfWork, AppDbContext>(sp =>
			sp.GetRequiredService<AppDbContext>());

		services.AddScoped<ISeederRunner, SeederRunner>();

		return services;
	}
}