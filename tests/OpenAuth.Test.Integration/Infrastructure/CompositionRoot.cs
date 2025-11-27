using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
using OpenAuth.Application.Tokens.Interfaces;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Services;
using OpenAuth.Domain.Users;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Infrastructure.Clients.Secrets;
using OpenAuth.Infrastructure.Clients.Secrets.Persistence;
using OpenAuth.Infrastructure.Identity;
using OpenAuth.Infrastructure.Keys.Jwks;
using OpenAuth.Infrastructure.OAuth.Jwt;
using OpenAuth.Infrastructure.OAuth.Persistence;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Infrastructure.Security.Hashing;
using OpenAuth.Infrastructure.SigningKeys.Jwk;
using OpenAuth.Infrastructure.SigningKeys.KeyMaterials;
using OpenAuth.Infrastructure.SigningKeys.Persistence;
using OpenAuth.Infrastructure.Tokens;
using OpenAuth.Infrastructure.Tokens.SigningCredentials;

namespace OpenAuth.Test.Integration.Infrastructure;

public class CompositionRoot
{
    public static ServiceProvider BuildService(string connectionString)
    {
        var builder = new ServiceCollection();

        builder.AddLogging();
        
        builder.AddDbContext<AppDbContext>(opts => 
            { opts.UseSqlServer(connectionString); });

        builder.AddIdentity<User, IdentityRole<Guid>>(options =>
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
        
        builder.AddSingleton(TimeProvider.System);

        // Client Services
        builder.AddScoped<IClientRepository, ClientRepository>();
        builder.AddScoped<IClientService, ClientService>();
        builder.AddScoped<IClientQueryService, ClientQueryService>();
        builder.AddScoped<IClientFactory, ClientFactory>();
        
        // Secret Services
        builder.AddScoped<ISecretService, SecretService>();
        builder.AddScoped<ISecretQueryService, SecretQueryService>();
        builder.AddScoped<IHasher, Pbkdf2Hasher>();
        builder.AddScoped<ISecretGenerator, SecretGenerator>();
        builder.AddScoped<ISecretHashProvider, SecretHashProvider>();

        // Signing Keys
        builder.AddScoped<ISigningKeyService, SigningKeyService>();
        builder.AddScoped<ISigningKeyQueryService, SigningKeyQueryService>();
        builder.AddScoped<ISigningKeyRepository, SigningKeyRepository>();
        
        builder.AddScoped<IKeyMaterialGenerator, HmacKeyMaterialGenerator>();
        builder.AddScoped<IKeyMaterialGenerator, RsaKeyMaterialGenerator>();
        builder.AddScoped<ISigningKeyFactory, SigningKeyFactory>();

        builder.AddScoped<ISigningCredentialsStrategy, HmacSigningCredentialsStrategy>();
        builder.AddScoped<ISigningCredentialsStrategy, RsaSigningCredentialsStrategy>();
        builder.AddScoped<ISigningCredentialsFactory, SigningCredentialsFactory>();
        
        // OAuth Services
        builder.AddScoped<ITokenRequestHandler, TokenRequestHandler>();
        builder.AddScoped<ITokenRequestProcessor, ClientCredentialsRequestProcessor>();
        builder.AddScoped<ITokenRequestProcessor, AuthorizationCodeProcessor>();

        builder.AddScoped<IAuthorizationCodeValidator, AuthorizationCodeValidator>();
        builder.AddScoped<IAuthorizationHandler, AuthorizationHandler>();
        builder.AddScoped<IAuthorizationRequestValidator, AuthorizationRequestValidator>();
        
        builder.AddScoped<IAuthorizationGrantStore, AuthorizationGrantStore>();
        builder.AddSingleton<ICache<AuthorizationGrant>, AuthorizationGrantCache>();
        
        // Jwks
        builder.AddScoped<IJwksService, JwksService>();
        builder.AddScoped<IPublicKeyInfoFactory, PublicKeyInfoFactory>();
        builder.AddScoped<IPublicKeyInfoExtractor, RsaPublicKeyInfoExtractor>();
        
        // Token Generator
        builder.Configure<JwtOptions>(opts => opts.Issuer = "test-issuer");
        builder.AddScoped<IJwtSigner, JwtSigner>();
        builder.AddScoped<IOidcUserClaimsProvider, OidcUserClaimsProvider>();
        builder.AddScoped<IJwtBuilderFactory, JwtBuilderFactory>();

        builder.AddScoped<ITokenHandler<AccessTokenContext>, AccessTokenHandler>();
        builder.AddScoped<ITokenHandler<IdTokenContext>, IdTokenHandler>();

        return builder.BuildServiceProvider();
    }
}