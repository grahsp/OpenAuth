using OpenAuth.AuthorizationApi.ApiResources;
using OpenAuth.AuthorizationApi.Connect.Authorize;
using OpenAuth.AuthorizationApi.Connect.Discovery;
using OpenAuth.AuthorizationApi.Connect.Jwks;
using OpenAuth.AuthorizationApi.Connect.Logout;
using OpenAuth.AuthorizationApi.Connect.Token;
using OpenAuth.AuthorizationApi.Connect.UserInfo;
using OpenAuth.Infrastructure.Modules;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.AuthorizationApi;

public class Program
{
	public static async Task Main(string[] args)
	{
		// Add services to the container.
		var builder = WebApplication.CreateBuilder(args);
		
		if (builder.Environment.IsDevelopment())
		{
			builder.Services.AddSwaggerGen();
			builder.Services.AddScoped<DataSeeder>();
		}

		builder.Services.AddRazorPages();
		builder.Services.AddEndpointsApiExplorer();

		builder.Services.AddLogging();

		builder.Services.AddCors(opts =>
		{
			opts.AddDefaultPolicy(policy =>
				policy.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowAnyOrigin()
			);
		});
        
		builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		
		// Infrastructure
		builder.Services.AddSingleton(TimeProvider.System);

		builder.Services
			.AddPersistenceModule(builder.Configuration)
			.AddIdentityModule();
		
		// OAuth / OIDC runtime
		builder.Services
			.AddTokenModule(builder.Configuration)
			.AddOAuthModule();
            
		// Authorization server configuration
		builder.Services
			.AddClientModule()
			.AddApiResourceModule()
			.AddSecretModule()
			.AddSigningKeyModule();

		// Configure the HTTP request pipeline.
		var app = builder.Build();

		app.UseCors();

		app.UseAuthentication();
		app.UseAuthorization();

		app.MapRazorPages();
		app.MapControllers();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();

			using var scope = app.Services.CreateScope();
			var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
			await seeder.SeedAsync(CancellationToken.None);
		}

		app.MapAuthorizeEndpoint();
		app.MapTokenEndpoint();
		app.MapLogoutEndpoint();

		app.MapUserInfoEndpoint();
		app.MapDiscoveryEndpoint();
		app.MapJwksEndpoint();
		
		app.MapApiEndpoints();
        
		app.Run();
	}
}