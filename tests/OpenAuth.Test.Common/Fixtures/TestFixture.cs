using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Test.Common.Helpers;
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