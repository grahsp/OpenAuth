using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Application.Clients.Factories;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Secrets.Services;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Factories;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.SigningKeys.Services;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.Services;
using OpenAuth.Infrastructure.ApiResources;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Infrastructure.Clients.Secrets;
using OpenAuth.Infrastructure.Clients.Secrets.Persistence;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Infrastructure.Security.Hashing;
using OpenAuth.Infrastructure.SigningKeys.Handlers;
using OpenAuth.Infrastructure.SigningKeys.KeyMaterials;
using OpenAuth.Infrastructure.SigningKeys.Persistence;
using OpenAuth.ManagementApi.Clients;

namespace OpenAuth.ManagementApi;

public class Program
{
    public static void Main(string[] args)
    {
        // Add services to the container.
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        
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

        // Database
        builder.Services.AddDbContext<AppDbContext>(opts =>
        {
            opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        
        builder.Services.AddSingleton(TimeProvider.System);

        // Client Services
        builder.Services.AddScoped<IClientRepository, ClientRepository>();
        builder.Services.AddScoped<IClientService, ClientService>();
        builder.Services.AddScoped<IClientQueryService, ClientQueryService>();
        builder.Services.AddScoped<IClientFactory, ClientFactory>();
        
        // ApiResource Services
        builder.Services.AddScoped<IApiResourceRepository, ApiResourceRepository>();
        
        // Secret Services
        builder.Services.AddScoped<ISecretService, SecretService>();
        builder.Services.AddScoped<ISecretQueryService, SecretQueryService>();
        builder.Services.AddScoped<IHasher, Pbkdf2Hasher>();
        builder.Services.AddScoped<ISecretGenerator, SecretGenerator>();
        builder.Services.AddScoped<ISecretHashProvider, SecretHashProvider>();

        // Signing Keys
        builder.Services.AddScoped<ISigningKeyService, SigningKeyService>();
        builder.Services.AddScoped<ISigningKeyQueryService, SigningKeyQueryService>();
        builder.Services.AddScoped<ISigningKeyRepository, SigningKeyRepository>();
        
        builder.Services.AddScoped<IKeyMaterialGenerator, HmacKeyMaterialGenerator>();
        builder.Services.AddScoped<IKeyMaterialGenerator, RsaKeyMaterialGenerator>();
        builder.Services.AddScoped<ISigningKeyFactory, SigningKeyFactory>();

        builder.Services.AddScoped<ISigningKeyHandler, HmacSigningKeyHandler>();
        builder.Services.AddScoped<ISigningKeyHandler, RsaSigningKeyHandler>();
        builder.Services.AddScoped<ISigningCredentialsFactory, SigningCredentialsFactory>();
        builder.Services.AddScoped<IValidationKeyFactory, ValidationKeyFactory>();
        
        // Configure the HTTP request pipeline.
        var app = builder.Build();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapClientEndpoints();
        
        app.Run();
    }
}
