using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Commands.CreateClient;
using OpenAuth.Application.Clients.Commands.GrantApiAccess;
using OpenAuth.Application.Clients.Commands.RevokeApiAccess;
using OpenAuth.Application.Clients.Commands.UpdateClient;
using OpenAuth.Application.Clients.Factories;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Queries.GetClientApiAccess;
using OpenAuth.Application.Clients.Queries.GetClientDetails;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Infrastructure.Clients;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Infrastructure.Clients.Queries;

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

		services.AddScoped<ICommandHandler<CreateClientCommand, CreateClientResult>, CreateClientCommandHandler>();
		services.AddScoped<ICommandHandler<CreateM2MClientCommand, CreateClientResult>, CreateM2MClientCommandHandler>();
		services.AddScoped<ICommandHandler<CreateSpaClientCommand, CreateClientResult>, CreateSpaClientCommandHandler>();
		services.AddScoped<ICommandHandler<CreateWebClientCommand, CreateClientResult>, CreateWebClientCommandHandler>();

		services.AddScoped<ICommandHandler<UpdateClientConfigurationCommand>, UpdateClientConfigurationCommandHandler>();
		
		services.AddScoped<ICommandHandler<GrantApiAccessCommand>, GrantApiAccessHandler>();
		services.AddScoped<ICommandHandler<RevokeApiAccessCommand>, RevokeApiAccessHandler>();

		return services;
	}


	public static IServiceCollection AddClientInfrastructure(this IServiceCollection services)
	{
		services.AddScoped<IClientRepository, ClientRepository>();
		services.AddScoped<IClientQueryService, ClientQueryService>();

		services.AddScoped<IQueryHandler<GetClientDetailsQuery, ClientDetails?>, GetClientDetailsQueryHandler>();
		services.AddScoped<IQueryHandler<GetClientPermissionsQuery, IReadOnlyList<ClientPermissions>>, GetClientPermissionsQueryHandler>();

		return services;
	}
}