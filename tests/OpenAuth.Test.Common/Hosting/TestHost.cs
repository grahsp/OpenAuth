using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Api;

namespace OpenAuth.Test.Common.Hosting;

public class TestHost(WebApplicationFactory<Program> factory)
{
	private IServiceProvider Services => factory.Services;
	public FakeTimeProvider Time => Resolve<FakeTimeProvider>();

	public HttpClient CreateClient() => factory.CreateClient();

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
	
	public T Resolve<T>() where T : notnull =>
		Services.GetRequiredService<T>();
	
	public Task<T> ResolveAsync<T>() where T : notnull =>
		WithScopeAsync(sp => Task.FromResult(sp.GetRequiredService<T>()));

	public async ValueTask DisposeAsync() => await factory.DisposeAsync();
}