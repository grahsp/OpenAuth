using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Commands.GrantApiAccess;
using OpenAuth.Application.Clients.Commands.RevokeApiAccess;
using OpenAuth.Application.Clients.Factories;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Infrastructure.Clients.Persistence;

namespace OpenAuth.Infrastructure.Modules;

public static class ClientModule
{
	public static IServiceCollection AddClientModule(this IServiceCollection services)
	{
		return services
			.AddClientApplication()
			.AddClientInfrastructure();
	}

	public static IServiceCollection AddClientApplication(this IServiceCollection services)
	{
		services.AddScoped<IClientService, ClientService>();
		services.AddScoped<IClientFactory, ClientFactory>();
		
		services.AddScoped<ICommandHandler<GrantApiAccessCommand>, GrantApiAccessHandler>();
		services.AddScoped<ICommandHandler<RevokeApiAccessCommand>, RevokeApiAccessHandler>();

		return services;
	}


	public static IServiceCollection AddClientInfrastructure(this IServiceCollection services)
	{
		services.AddScoped<IClientRepository, ClientRepository>();
		services.AddScoped<IClientQueryService, ClientQueryService>();

		return services;
	}
}