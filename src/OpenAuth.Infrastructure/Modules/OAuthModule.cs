using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.OAuth.Stores;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Infrastructure.OAuth.Persistence;

namespace OpenAuth.Infrastructure.Modules;

public static class OAuthModule
{
	public static IServiceCollection AddOAuthModule(this IServiceCollection services)
	{
		return services
			.AddOAuthApplication()
			.AddOAuthInfrastructure();
	}
    
	public static IServiceCollection AddOAuthApplication(this IServiceCollection services)
	{
		services.AddScoped<ITokenRequestHandler, TokenRequestHandler>();
		services.AddScoped<ITokenRequestProcessor, ClientCredentialsRequestProcessor>();
        
		services.AddScoped<ITokenRequestProcessor, AuthorizationCodeProcessor>();
		services.AddScoped<IAuthorizationCodeValidator, AuthorizationCodeValidator>();

		services.AddScoped<IAuthorizationHandler, AuthorizationHandler>();
		services.AddScoped<IAuthorizationRequestValidator, AuthorizationRequestValidator>();
        
		services.AddScoped<IAuthorizationGrantStore, AuthorizationGrantStore>();
        
		return services;
	}

	public static IServiceCollection AddOAuthInfrastructure(this IServiceCollection services)
	{
		services.AddSingleton<ICache<AuthorizationGrant>, AuthorizationGrantCache>();

		return services;
	}
}