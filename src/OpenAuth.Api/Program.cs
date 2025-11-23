using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Api.Connect.Authorize;
using OpenAuth.Api.Connect.Token;
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
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Factories;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.SigningKeys.Services;
using OpenAuth.Application.Tokens;
using OpenAuth.Application.Tokens.Configurations;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Application.Tokens.Interfaces;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Services;
using OpenAuth.Domain.Users;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Infrastructure.Clients.Secrets;
using OpenAuth.Infrastructure.Clients.Secrets.Persistence;
using OpenAuth.Infrastructure.Keys.Jwks;
using OpenAuth.Infrastructure.OAuth.Persistence;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Infrastructure.Security.Hashing;
using OpenAuth.Infrastructure.SigningKeys.Jwk;
using OpenAuth.Infrastructure.SigningKeys.KeyMaterials;
using OpenAuth.Infrastructure.SigningKeys.Persistence;
using OpenAuth.Infrastructure.Tokens;
using OpenAuth.Infrastructure.Tokens.SigningCredentials;

namespace OpenAuth.Api;

public class Program
{
    public static void Main(string[] args)
    {
        // Add services to the container.
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorPages();
        builder.Services.AddControllers();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Database
        builder.Services.AddDbContext<AppDbContext>(opts =>
        {
            opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
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
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/account/login";
            options.LogoutPath = "/account/logout";
            options.AccessDeniedPath = "/account/access_denied";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.SlidingExpiration = true;
        });
        
        builder.Services.AddSingleton(TimeProvider.System);

        // Client Services
        builder.Services.AddScoped<IClientRepository, ClientRepository>();
        builder.Services.AddScoped<IClientService, ClientService>();
        builder.Services.AddScoped<IClientQueryService, ClientQueryService>();
        builder.Services.AddScoped<IClientFactory, ClientFactory>();
        
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
        builder.Services.AddScoped<IJwtBuilderFactory, JwtBuilderFactory>();
        

        // Configure the HTTP request pipeline.
        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapAuthorizeEndpoint();
        app.MapTokenEndpoint();
        
        app.Run();
    }
}