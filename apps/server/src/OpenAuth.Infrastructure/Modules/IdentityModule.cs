using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Domain.Users;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.Modules;

public static class IdentityModule
{
	public static IServiceCollection AddIdentityModule(this IServiceCollection services)
	{
		return services
			.AddIdentityApplication()
			.AddIdentityInfrastructure();
	}

	public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
	{
		services.AddScoped<SignInManager<User>>();
		
		return services;
	}

	public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
	{
		services.AddAuthentication(IdentityConstants.ApplicationScheme)
			.AddCookie(IdentityConstants.ApplicationScheme);
		
		services.AddIdentityCore<User>(options =>
			{
				// Password settings
				options.Password.RequireDigit = false;
				options.Password.RequireLowercase = false;
				options.Password.RequireUppercase = false;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequiredLength = 4;
				options.Password.RequiredUniqueChars = 0;
            
				// Lockout settings
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;
            
				// User settings
				options.User.RequireUniqueEmail = true;

				// Sign in settings
				options.SignIn.RequireConfirmedEmail = false;
			})
			.AddRoles<IdentityRole<Guid>>()
			.AddEntityFrameworkStores<AppDbContext>()
			.AddDefaultTokenProviders();

		services.ConfigureApplicationCookie(options =>
		{
			options.LoginPath = "/account/login";
			options.LogoutPath = "/account/logout";
			options.AccessDeniedPath = "/account/access_denied";
			options.Cookie.HttpOnly = true;
			options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
			options.ExpireTimeSpan = TimeSpan.FromHours(1);
			options.SlidingExpiration = true;
		});

		return services;
	}
}