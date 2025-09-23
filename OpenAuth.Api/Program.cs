using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Clients;
using OpenAuth.Application.Security.Keys;
using OpenAuth.Application.Security.Secrets;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.Security.Tokens;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.Configurations;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Infrastructure.Repositories;
using OpenAuth.Infrastructure.Security.Keys;
using OpenAuth.Infrastructure.Security.Secrets;
using OpenAuth.Infrastructure.Security.Signing;
using OpenAuth.Infrastructure.Security.Tokens;

namespace OpenAuth.Api;

public class Program
{
    public static void Main(string[] args)
    {
        // Add services to the container.
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Database
        builder.Services.AddDbContext<AppDbContext>(opts =>
        {
            opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        // Client Services
        builder.Services.AddScoped<IClientRepository, ClientRepository>();
        builder.Services.AddScoped<IClientSecretFactory, ClientSecretFactory>();
        builder.Services.AddScoped<IClientSecretValidator, ClientSecretValidator>();
        builder.Services.AddScoped<IClientService, ClientService>();
        
        // Client Secret
        builder.Services.AddScoped<ISecretHasher, Pbkdf2Hasher>();
        builder.Services.AddScoped<ISecretGenerator, SecretGenerator>();

        // Signing Keys
        builder.Services.AddScoped<ISigningKeyService, SigningKeyService>();
        builder.Services.AddScoped<ISigningKeyRepository, SigningKeyRepository>();
        builder.Services.AddScoped<IKeyParameterExporter, RsaKeyParameterExporter>();
        
        builder.Services.AddScoped<ISigningKeyStrategy, HmacSigningKeyStrategy>();
        builder.Services.AddScoped<ISigningKeyStrategy, RsaSigningKeyStrategy>();
        builder.Services.AddScoped<ISigningKeyFactory, SigningKeyFactory>();

        builder.Services.AddScoped<ISigningCredentialsStrategy, HmacSigningCredentialsStrategy>();
        builder.Services.AddScoped<ISigningCredentialsStrategy, RsaSigningCredentialsStrategy>();
        builder.Services.AddScoped<ISigningCredentialsFactory, SigningCredentialsFactory>();
        
        // Token Generator
        builder.Services.Configure<Auth>(builder.Configuration.GetSection("Auth"));
        builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        

        // Configure the HTTP request pipeline.
        var app = builder.Build();

        app.MapControllers();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.Run();
    }
}