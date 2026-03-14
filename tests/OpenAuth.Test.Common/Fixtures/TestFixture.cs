using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenAuth.Application.SigningKeys.Factories;
using OpenAuth.Infrastructure.Clients.Secrets;
using OpenAuth.Test.Common.Fakes;
using OpenAuth.Test.Common.Hosting;
using OpenAuth.Test.Common.Infrastructure;
using Xunit;

namespace OpenAuth.Test.Common.Fixtures;

public sealed class TestFixture : IAsyncLifetime
{
	private SqlServer _database = null!;

	public async Task InitializeAsync()
	{
		_database = new SqlServer();
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
				services.RemoveAll<ISecretGenerator>();
				services.AddSingleton<ISecretGenerator, FakeGenerator>(_ => new FakeGenerator("secret"));
				
				services.RemoveAll<ISigningKeyFactory>();
				services.AddSingleton<ISigningKeyFactory, FakeSigningKeyFactory>();
			});
            
			configure?.Invoke(builder);
		});
	}

	public async Task ResetAsync() => await _database.ResetAsync();

	public async Task DisposeAsync() => await _database.DisposeAsync();
}