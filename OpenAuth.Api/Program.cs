using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Audiences.Services;
using OpenAuth.Application.Clients.Factories;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Application.Jwks.Services;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.OAuth.Stores;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Secrets.Services;
using OpenAuth.Application.Security.Hashing;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Factories;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.SigningKeys.Services;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Application.Tokens.Interfaces;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Application.Users.Interfaces;
using OpenAuth.Application.Users.Services;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Infrastructure.Clients.Secrets;
using OpenAuth.Infrastructure.Clients.Secrets.Persistence;
using OpenAuth.Infrastructure.Configurations;
using OpenAuth.Infrastructure.Keys.Jwks;
using OpenAuth.Infrastructure.OAuth.Persistence;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Infrastructure.Security.Hashing;
using OpenAuth.Infrastructure.SigningKeys.Jwk;
using OpenAuth.Infrastructure.SigningKeys.KeyMaterials;
using OpenAuth.Infrastructure.SigningKeys.Persistence;
using OpenAuth.Infrastructure.Tokens;
using OpenAuth.Infrastructure.Tokens.SigningCredentials;
using OpenAuth.Infrastructure.Users.Persistence;

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
        builder.Services.AddScoped<IClientService, ClientService>();
        builder.Services.AddScoped<IClientQueryService, ClientQueryService>();
        builder.Services.AddScoped<IClientFactory, ClientFactory>();
        
        // Audience Services
        builder.Services.AddScoped<IAudienceService, AudienceService>();
        
        // Secret Services
        builder.Services.AddScoped<ISecretService, SecretService>();
        builder.Services.AddScoped<ISecretQueryService, SecretQueryService>();
        builder.Services.AddScoped<IHasher, Pbkdf2Hasher>();
        builder.Services.AddScoped<ISecretGenerator, SecretGenerator>();

        // Signing Keys
        builder.Services.AddScoped<ISigningKeyService, SigningKeyService>();
        builder.Services.AddScoped<ISigningKeyQueryService, SigningKeyQueryService>();
        builder.Services.AddScoped<ISigningKeyRepository, SigningKeyRepository>();
        
        builder.Services.AddScoped<IKeyMaterialGenerator, HmacKeyMaterialGenerator>();
        builder.Services.AddScoped<IKeyMaterialGenerator, RsaKeyMaterialGenerator>();
        builder.Services.AddScoped<ISigningKeyFactory, SigningKeyFactory>();

        builder.Services.AddScoped<ISigningCredentialsStrategy, HmacSigningCredentialsStrategy>();
        builder.Services.AddScoped<ISigningCredentialsStrategy, RsaSigningCredentialsStrategy>();
        builder.Services.AddScoped<ISigningCredentialsFactory, SigningCredentialsFactory>();
        
        // OAuth Services
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<ITokenIssuer, ClientCredentialsTokenIssuer>();
        builder.Services.AddScoped<ITokenIssuer, AuthorizationCodeTokenIssuer>();

        builder.Services.AddScoped<IAuthorizationHandler, AuthorizationHandler>();
        
        builder.Services.AddScoped<IAuthorizationGrantStore, AuthorizationGrantStore>();
        builder.Services.AddSingleton<ICache<AuthorizationGrant>, AuthorizationGrantCache>();
        
        // Jwks
        builder.Services.AddScoped<IJwksService, JwksService>();
        builder.Services.AddScoped<IPublicKeyInfoFactory, PublicKeyInfoFactory>();
        builder.Services.AddScoped<IPublicKeyInfoExtractor, RsaPublicKeyInfoExtractor>();
        
        // Token Generator
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Auth"));
        builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        
        // User
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserService, UserService>();
        

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