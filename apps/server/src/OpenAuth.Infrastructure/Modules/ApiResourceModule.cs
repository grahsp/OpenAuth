using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.ApiResources.Commands.AddApiResourcePermissions;
using OpenAuth.Application.ApiResources.Commands.CreateApiResource;
using OpenAuth.Application.ApiResources.Commands.DeleteApiResource;
using OpenAuth.Application.ApiResources.Commands.RemoveApiResourcePermissions;
using OpenAuth.Application.ApiResources.Queries.GetApiDetails;
using OpenAuth.Application.ApiResources.Queries.GetApiSummaryList;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Infrastructure.ApiResources;
using OpenAuth.Infrastructure.ApiResources.Queries.GetApiDetails;
using OpenAuth.Infrastructure.ApiResources.Queries.GetApiSummaryList;

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
		services.AddScoped<IQueryHandler<GetApiSummaryListQuery, IReadOnlyList<ApiSummaryView>>, GetApiSummaryListQueryHandler>();
		services.AddScoped<IQueryHandler<GetApiDetailsQuery, ApiDetailsView>, GetApiDetailsQueryHandler>();
		
		services.AddScoped<ICommandHandler<CreateApiResourceCommand, CreateApiResourceResult>, CreateApiResourceHandler>();
		services.AddScoped<ICommandHandler<DeleteApiResourceCommand>, DeleteApiResourceHandler>();
		
		services.AddScoped<ICommandHandler<AddApiResourcePermissionCommand>, AddApiResourcePermissionsHandler>();
		services.AddScoped<ICommandHandler<RemoveApiResourcePermissionsCommand>, RemoveApiResourcePermissionsHandler>();
		
		return services;
	}

	public static IServiceCollection AddApiResourceInfrastructure(this IServiceCollection services)
	{
		services.AddScoped<IApiResourceRepository, ApiResourceRepository>();

		return services;
	}
}