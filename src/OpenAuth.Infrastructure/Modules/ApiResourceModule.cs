using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.ApiResources.Commands.AddApiResourcePermissions;
using OpenAuth.Application.ApiResources.Commands.CreateApiResource;
using OpenAuth.Application.ApiResources.Commands.DeleteApiResource;
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
		services.AddScoped<ICommandHandler<CreateApiResourceCommand, CreateApiResourceResult>, CreateApiResourceHandler>();
		services.AddScoped<ICommandHandler<DeleteApiResourceCommand>, DeleteApiResourceHandler>();
		
		services.AddScoped<ICommandHandler<AddApiResourcePermissionCommand>, AddApiResourcePermissionsHandler>();
		
		return services;
	}

	public static IServiceCollection AddApiResourceInfrastructure(this IServiceCollection services)
	{
		services.AddScoped<IApiResourceRepository, ApiResourceRepository>();

		return services;
	}
}