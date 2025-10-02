using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Clients;
using OpenAuth.Application.Security.Jwks;
using OpenAuth.Application.Security.Keys;
using OpenAuth.Application.Security.Secrets;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.Security.Tokens;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.Configurations;
using OpenAuth.Infrastructure.Clients;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Infrastructure.Repositories;
using OpenAuth.Infrastructure.Security.Jwks;
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
        
        builder.Services.AddSingleton(TimeProvider.System);

        // Client Services
        builder.Services.AddScoped<IClientRepository, ClientRepository>();
        builder.Services.AddScoped<IClientSecretValidator, ClientSecretValidator>();
        builder.Services.AddScoped<IClientService, ClientService>();
        builder.Services.AddScoped<IClientFactory, ClientFactory>();
        
        // Secret Services
        builder.Services.AddScoped<ISecretService, SecretService>();
        builder.Services.AddScoped<ISecretHasher, Pbkdf2Hasher>();
        builder.Services.AddScoped<ISecretGenerator, SecretGenerator>();

        // Signing Keys
        builder.Services.AddScoped<ISigningKeyService, SigningKeyService>();
        builder.Services.AddScoped<ISigningKeyRepository, SigningKeyRepository>();
        builder.Services.AddScoped<IPublicKeyInfoExtractor, RsaPublicKeyInfoExtractor>();
        
        builder.Services.AddScoped<IKeyMaterialGenerator, HmacKeyMaterialGenerator>();
        builder.Services.AddScoped<IKeyMaterialGenerator, RsaKeyMaterialGenerator>();
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