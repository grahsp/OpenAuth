using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenAuth.AuthorizationApi.Connect.Authorize;
using OpenAuth.AuthorizationApi.Connect.Discovery;
using OpenAuth.AuthorizationApi.Connect.Jwks;
using OpenAuth.AuthorizationApi.Connect.Logout;
using OpenAuth.AuthorizationApi.Connect.Token;
using OpenAuth.AuthorizationApi.Connect.UserInfo;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Application.Jwks.Services;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Application.OAuth.Services;
using OpenAuth.Application.OAuth.Stores;
using OpenAuth.Application.Oidc;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.Tokens;
using OpenAuth.Application.Tokens.Configurations;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.AuthorizationApi.Http;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Services;
using OpenAuth.Domain.Users;
using OpenAuth.Infrastructure.ApiResources;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Infrastructure.Clients.Secrets.Persistence;
using OpenAuth.Infrastructure.Identity;
using OpenAuth.Infrastructure.OAuth.Jwt;
using OpenAuth.Infrastructure.OAuth.Persistence;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Infrastructure.Security.Hashing;
using OpenAuth.Infrastructure.SigningKeys.Jwks;
using OpenAuth.Infrastructure.SigningKeys.Persistence;

namespace OpenAuth.AuthorizationApi;

public class Program
{
    public static void Main(string[] args)
    {
        // Add services to the container.
        var builder = WebApplication.CreateBuilder(args);
        
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        builder.Services.AddRazorPages();
        
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
        builder.Services.AddScoped<IClientQueryService, ClientQueryService>();
        
        // ApiResource Services
        builder.Services.AddScoped<IApiResourceRepository, ApiResourceRepository>();
        
        // Secret Services
        builder.Services.AddScoped<ISecretQueryService, SecretQueryService>();
        builder.Services.AddScoped<IHasher, Pbkdf2Hasher>();

        // Signing Keys
        builder.Services.AddScoped<ISigningKeyQueryService, SigningKeyQueryService>();
        builder.Services.AddScoped<ISigningCredentialsFactory, SigningCredentialsFactory>();
        builder.Services.AddScoped<IValidationKeyFactory, ValidationKeyFactory>();
        
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

        builder.Services.AddScoped<IBearerTokenExtractor, BearerTokenExtractor>();
        builder.Services.AddScoped<IAccessTokenValidator, AccessTokenValidator>();
        builder.Services.AddScoped<IUserInfoService, UserInfoService>();
        

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
        
        app.Run();
    }
}