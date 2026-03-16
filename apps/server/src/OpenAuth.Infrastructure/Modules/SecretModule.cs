using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Secrets.Services;
using OpenAuth.Domain.Services;
using OpenAuth.Infrastructure.Clients.Secrets;
using OpenAuth.Infrastructure.Clients.Secrets.Persistence;
using OpenAuth.Infrastructure.Security.Hashing;

namespace OpenAuth.Infrastructure.Modules;

public static class SecretModule
{
	public static IServiceCollection AddSecretModule(this IServiceCollection services)
	{
		return services
			.AddSecretApplication()
			.AddSecretInfrastructure();
	}

	public static IServiceCollection AddSecretApplication(this IServiceCollection services)
	{
		services.AddScoped<ISecretService, SecretService>();

		return services;
	}

	public static IServiceCollection AddSecretInfrastructure(this IServiceCollection services)
	{
		services.AddScoped<ISecretQueryService, SecretQueryService>();
		services.AddScoped<ISecretGenerator, SecretGenerator>();
		services.AddScoped<IHasher, Pbkdf2Hasher>();
		services.AddScoped<ISecretHashProvider, SecretHashProvider>();

		return services;
	}
}