using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Application.Clients.Factories;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.Apis;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Services;
using OpenAuth.Infrastructure.ApiResources;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Infrastructure.Clients.Secrets;
using OpenAuth.Infrastructure.SigningKeys.Handlers;
using OpenAuth.Test.Common.Helpers;
using OpenAuth.Test.Common.Hosting;
using OpenAuth.Test.Common.Infrastructure;
using Xunit;

namespace OpenAuth.Test.Common.Fixtures;

public sealed class TestFixture : IAsyncLifetime
{
	private readonly SqlServer _database = new SqlServer();

	public async Task InitializeAsync()
	{
		await _database.InitializeAsync();
	}

	public TestHost CreateHost(Action<TestHostBuilder>? configure = null)
	{
		return TestHostFactory.Create(builder =>
		{
			builder.UseDatabase(options => options.UseSqlServer(_database.ConnectionString));

			builder.ConfigureServices(services =>
			{
				services.AddAuthentication(opts =>
					{
						opts.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
						opts.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
						opts.DefaultScheme = TestAuthenticationHandler.SchemeName;
					})
					.AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
						TestAuthenticationHandler.SchemeName, _ => { });
				
				services.AddAuthorization();

				// To create clients
				services.AddScoped<IClientService, ClientService>();
				services.AddScoped<IClientFactory, ClientFactory>();
				services.AddScoped<IClientRepository, ClientRepository>();
				services.AddScoped<IApiResourceRepository, ApiResourceRepository>();

				services.AddScoped<ISecretHashProvider, SecretHashProvider>();
				services.AddScoped<ISecretGenerator, SecretGenerator>();

				services.AddScoped<ISigningCredentialsFactory, SigningCredentialsFactory>();
				services.AddScoped<ISigningKeyHandler, RsaSigningKeyHandler>();
        
				services.AddScoped<IValidationKeyFactory, ValidationKeyFactory>();
			});
            
			configure?.Invoke(builder);
		});
	}

	public TestHost CreateDefaultHost(Action<TestHostBuilder>? configure = null)
	{
		return CreateHost(builder =>
		{
			builder.ConfigureServices(services =>
			{
			});
            
			configure?.Invoke(builder);
		});
	}

	public async Task ResetAsync(TestHost host)
	{
		await _database.ResetAsync();
		
		await using var scope = host.CreateScope();
		await Given.SigningKeyAsync(scope);
	}

	public async Task DisposeAsync() => await _database.DisposeAsync();
}