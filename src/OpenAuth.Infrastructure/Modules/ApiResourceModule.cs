using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Infrastructure.ApiResources;

namespace OpenAuth.Infrastructure.Modules;

public static class ApiResourceModule
{
	public static IServiceCollection AddApiResourceModule(this IServiceCollection services)
	{
		return services
			.AddApiResourceApplication()
			.AddApiResourceInfrastructure();
	}

	public static IServiceCollection AddApiResourceApplication(this IServiceCollection services)
	{
		return services;
	}

	public static IServiceCollection AddApiResourceInfrastructure(this IServiceCollection services)
	{
		services.AddScoped<IApiResourceRepository, ApiResourceRepository>();

		return services;
	}
}