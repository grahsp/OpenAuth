using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Api;
using OpenAuth.Application.SigningKeys.Services;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.Users;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Hosting;

public class TestHost(WebApplicationFactory<Program> factory)
{
	private IServiceProvider Services => factory.Services;
	public FakeTimeProvider Time => Resolve<FakeTimeProvider>();

	public HttpClient CreateClient() => factory.CreateClient(new WebApplicationFactoryClientOptions
	{
		AllowAutoRedirect = false
	});

	public TOptions GetOptions<TOptions>() where TOptions : class =>
		factory.Services.GetRequiredService<IOptions<TOptions>>().Value;
	
	public async Task WithScopeAsync(Func<IServiceProvider, Task> func)
	{
		await using var scope = Services.CreateAsyncScope();
		await func(scope.ServiceProvider);
	}

	public async Task<T> WithScopeAsync<T>(Func<IServiceProvider, Task<T>> func)
	{
		await using var scope = Services.CreateAsyncScope();
		return await func(scope.ServiceProvider);
	}

	public TModule CreateModuleAsync<TModule>(Func<IServiceScope, TModule> func)
	{
		var scope = Services.CreateAsyncScope();
		return func(scope);
	}

	public async Task SeedSigningKeyAsync()
	{
		await WithScopeAsync(async sp =>
		{
			var signing = sp.GetRequiredService<ISigningKeyService>();
			await signing.CreateAsync(SigningAlgorithm.RS256, TimeSpan.FromDays(7000));
		});
	}

	public async Task SeedTestUserAsync()
	{
		await WithScopeAsync(async sp =>
		{
			var user = new User
			{
				Id = Guid.Parse(DefaultValues.UserId),
				UserName = DefaultValues.UserName,
				Email = DefaultValues.UserEmail
			};
			
			var manager = sp.GetRequiredService<UserManager<User>>();
			await manager.CreateAsync(user, DefaultValues.UserPassword);
		});
	}
	
	public T Resolve<T>() where T : notnull =>
		Services.GetRequiredService<T>();
	
	public Task<T> ResolveAsync<T>() where T : notnull =>
		WithScopeAsync(sp => Task.FromResult(sp.GetRequiredService<T>()));

	public async ValueTask DisposeAsync() => await factory.DisposeAsync();
}