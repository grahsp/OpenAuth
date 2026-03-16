using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Application.Jwks.Services;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Factories;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.SigningKeys.Services;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Infrastructure.SigningKeys.Handlers;
using OpenAuth.Infrastructure.SigningKeys.Jwks;
using OpenAuth.Infrastructure.SigningKeys.KeyMaterials;
using OpenAuth.Infrastructure.SigningKeys.Persistence;

namespace OpenAuth.Infrastructure.Modules;

public static class SigningKeyModule
{
	public static IServiceCollection AddSigningKeyModule(this IServiceCollection services)
	{
		return services
			.AddSigningKeyApplication()
			.AddSigningKeyInfrastructure();
		
	}

	public static IServiceCollection AddSigningKeyApplication(this IServiceCollection services)
	{
		services.AddScoped<ISigningKeyService, SigningKeyService>();
		services.AddScoped<ISigningKeyFactory, SigningKeyFactory>();
		services.AddScoped<ISigningCredentialsFactory, SigningCredentialsFactory>();
		services.AddScoped<IValidationKeyFactory, ValidationKeyFactory>();
		
		services.AddScoped<IJwksService, JwksService>();

		return services;
	}

	public static IServiceCollection AddSigningKeyInfrastructure(this IServiceCollection services)
	{
		services.AddScoped<ISigningKeyQueryService, SigningKeyQueryService>();
		services.AddScoped<ISigningKeyRepository, SigningKeyRepository>();

		services.AddScoped<IKeyMaterialGenerator, HmacKeyMaterialGenerator>();
		services.AddScoped<IKeyMaterialGenerator, RsaKeyMaterialGenerator>();

		services.AddScoped<ISigningKeyHandler, HmacSigningKeyHandler>();
		services.AddScoped<ISigningKeyHandler, RsaSigningKeyHandler>();
		
		services.AddScoped<IJwkFactory, JwkFactory>();

		return services;
	}
}