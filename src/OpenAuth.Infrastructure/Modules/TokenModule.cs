using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Application.OAuth.Services;
using OpenAuth.Application.Oidc;
using OpenAuth.Application.Tokens;
using OpenAuth.Application.Tokens.Configurations;
using OpenAuth.Application.Tokens.Interfaces;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Infrastructure.Identity;
using OpenAuth.Infrastructure.OAuth.Jwt;
using OpenAuth.Infrastructure.Tokens;

namespace OpenAuth.Infrastructure.Modules;

public static class TokenModule
{
	public static IServiceCollection AddTokenModule(this IServiceCollection services, IConfiguration configuration)
	{
		return services
			.AddTokenApplication(configuration)
			.AddTokenInfrastructure();
	}
    
	public static IServiceCollection AddTokenApplication(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<JwtOptions>(configuration.GetSection("Auth"));

		services.AddScoped<IAccessTokenValidator, AccessTokenValidator>();
        
		services.AddScoped<ITokenHandler<AccessTokenContext>, AccessTokenHandler>();
		services.AddScoped<ITokenHandler<IdTokenContext>, IdTokenHandler>();
        
		services.AddScoped<IUserInfoService, UserInfoService>();
		services.AddScoped<IOidcUserClaimsProvider, OidcUserClaimsProvider>();
        
		return services;
	}

	public static IServiceCollection AddTokenInfrastructure(this IServiceCollection services)
	{
		JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        
		services.AddScoped<IJwtSigner, JwtSigner>();
		services.AddScoped<IBearerTokenExtractor, BearerTokenExtractor>();

		return services;
	}
}