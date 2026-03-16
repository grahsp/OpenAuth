using OpenAuth.Infrastructure.Modules;
using OpenAuth.ManagementApi.Clients;
using OpenAuth.ManagementApi.Secrets;
using OpenAuth.ManagementApi.SigningKeys;

namespace OpenAuth.ManagementApi;

public class Program
{
	public static void Main(string[] args)
	{
		// Add services to the container.
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		builder.Services.AddLogging();
		
		builder.Services.AddCors(opts =>
		{
			opts.AddDefaultPolicy(policy =>
				policy.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowAnyOrigin()
			);
		});
		
		builder.Services.AddSingleton(TimeProvider.System);
		
		builder.Services
			.AddPersistenceModule(builder.Configuration)
			.AddClientModule()
			.AddApiResourceModule()
			.AddSecretModule()
			.AddSigningKeyModule();
		
		// Configure the HTTP request pipeline.
		var app = builder.Build();

		app.UseCors();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.MapClientEndpoints();
		app.MapSigningKeyEndpoints();
		app.MapSecretEndpoints();
        
		app.Run();
	}
}