using OpenAuth.Server.ApiResources;
using OpenAuth.Server.Clients;
using OpenAuth.Server.Connect.Authorize;
using OpenAuth.Server.Connect.Discovery;
using OpenAuth.Server.Connect.Jwks;
using OpenAuth.Server.Connect.Logout;
using OpenAuth.Server.Connect.Token;
using OpenAuth.Server.Connect.UserInfo;
using OpenAuth.Server.Secrets;
using OpenAuth.Server.SigningKeys;
using OpenAuth.Infrastructure.Modules;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Server;

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
		
		app.MapClientEndpoints();
		app.MapApiEndpoints();
		app.MapSigningKeyEndpoints();
		app.MapSecretEndpoints();
        
		app.Run();
	}
}