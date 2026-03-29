using Microsoft.AspNetCore.HttpOverrides;
using OpenAuth.Infrastructure.Modules;
using OpenAuth.Infrastructure.Persistence.Seeders;
using OpenAuth.Server.Connect;
using OpenAuth.Server.Discovery;
using OpenAuth.Server.Extensions;
using OpenAuth.Server.Management;

namespace OpenAuth.Server;

public class Program
{
	public static async Task Main(string[] args)
	{
		// Add services to the container.
		var builder = WebApplication.CreateBuilder(args);
		
		builder.Services.Configure<ForwardedHeadersOptions>(options =>
		{
			options.ForwardedHeaders =
				ForwardedHeaders.XForwardedFor |
				ForwardedHeaders.XForwardedProto |
				ForwardedHeaders.XForwardedHost;

			// allow docker/nginx
			options.KnownNetworks.Clear();
			options.KnownProxies.Clear();
		});
		
		if (builder.Environment.IsDevelopment())
			builder.Services.AddSwaggerGen();

		builder.Services.AddRazorPages();
		builder.Services.AddEndpointsApiExplorer();

		builder.Services.AddLogging();

		builder.Services.AddCors(opts =>
		{
			opts.AddPolicy("Public", policy =>
				policy.AllowAnyOrigin()
					.AllowAnyHeader()
					.AllowAnyMethod()
			);
			
			opts.AddPolicy("Dashboard", policy =>
				policy.WithOrigins("http://localhost:5173")
					.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowCredentials()
			);
		});
        
		builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		
		// Infrastructure
		builder.Services.AddSingleton(TimeProvider.System);

		builder.Services
			.AddPersistenceModule(builder.Configuration)
			.AddIdentityModule(builder.Configuration);
		
		// OAuth / OIDC runtime
		builder.Services
			.AddTokenModule()
			.AddOAuthModule(builder.Configuration);
            
		// Authorization server configuration
		builder.Services
			.AddClientModule()
			.AddApiResourceModule()
			.AddSecretModule()
			.AddSigningKeyModule();
		
		if (builder.Configuration.GetValue<bool>("Seed:Demo"))
			builder.Services.AddScoped<ISeeder, DemoSeeder>();
		

		// Configure the HTTP request pipeline.
		var app = builder.Build();

		app.UseForwardedHeaders();

		app.UseCors("Public");
		app.UseCors("Dashboard");

		app.UseAuthentication();
		app.UseAuthorization();

		app.MapRazorPages();
		app.MapControllers();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		var ct = app.Lifetime.ApplicationStopping;
		
		await app.InitializeDatabaseAsync(ct);
		await app.SeedAsync(ct);

		app.MapConnectEndpoints();
		app.MapDiscoveryEndpoints();
		app.MapManagementEndpoints();
        
		await app.RunAsync();
	}
}