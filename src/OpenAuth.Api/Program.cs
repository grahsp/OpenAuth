using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Api.Clients;
using OpenAuth.Api.Connect.Authorize;
using OpenAuth.Api.Connect.Discovery;
using OpenAuth.Api.Connect.Jwks;
using OpenAuth.Api.Connect.Logout;
using OpenAuth.Api.Connect.Token;
using OpenAuth.Api.Connect.UserInfo;
using OpenAuth.Application.Clients.Factories;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Application.Jwks.Services;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Application.OAuth.Stores;
using OpenAuth.Application.Oidc;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Secrets.Services;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Factories;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.SigningKeys.Services;
using OpenAuth.Application.Tokens;
using OpenAuth.Application.Tokens.Configurations;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Services;
using OpenAuth.Domain.Users;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Infrastructure.Clients.Secrets;
using OpenAuth.Infrastructure.Clients.Secrets.Persistence;
using OpenAuth.Infrastructure.Identity;
using OpenAuth.Infrastructure.OAuth.Jwt;
using OpenAuth.Infrastructure.OAuth.Persistence;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Infrastructure.Security.Hashing;
using OpenAuth.Infrastructure.SigningKeys.Handlers;
using OpenAuth.Infrastructure.SigningKeys.Jwks;
using OpenAuth.Infrastructure.SigningKeys.KeyMaterials;
using OpenAuth.Infrastructure.SigningKeys.Persistence;

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

        builder.Services.AddScoped<ISigningKeyHandler, HmacSigningKeyHandler>();
        builder.Services.AddScoped<ISigningKeyHandler, RsaSigningKeyHandler>();
        builder.Services.AddScoped<ISigningCredentialsFactory, SigningCredentialsFactory>();
        
        // OAuth Services
        builder.Services.AddScoped<ITokenRequestHandler, TokenRequestHandler>();
        builder.Services.AddScoped<ITokenRequestProcessor, ClientCredentialsRequestProcessor>();
        builder.Services.AddScoped<ITokenRequestProcessor, AuthorizationCodeProcessor>();

        builder.Services.AddScoped<IAuthorizationCodeValidator, AuthorizationCodeValidator>();
        builder.Services.AddScoped<IAuthorizationHandler, AuthorizationHandler>();
        builder.Services.AddScoped<IAuthorizationRequestValidator, AuthorizationRequestValidator>();
        
        builder.Services.AddScoped<IAuthorizationGrantStore, AuthorizationGrantStore>();
        builder.Services.AddSingleton<ICache<AuthorizationGrant>, AuthorizationGrantCache>();
        
        // Jwks
        builder.Services.AddScoped<IJwksService, JwksService>();
        builder.Services.AddScoped<IJwkFactory, JwkFactory>();
        
        // Token Generator
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Auth"));
        builder.Services.AddScoped<IJwtSigner, JwtSigner>();
        builder.Services.AddScoped<IOidcUserClaimsProvider, OidcUserClaimsProvider>();
        
        builder.Services.AddScoped<ITokenHandler<AccessTokenContext>, AccessTokenHandler>();
        builder.Services.AddScoped<ITokenHandler<IdTokenContext>, IdTokenHandler>();
        

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
        }

        app.MapAuthorizeEndpoint();
        app.MapTokenEndpoint();
        app.MapLogoutEndpoint();

        app.MapUserInfoEndpoint();
        app.MapDiscoveryEndpoint();
        app.MapJwksEndpoint();

        app.MapClientEndpoints();
        
        app.Run();
    }
}